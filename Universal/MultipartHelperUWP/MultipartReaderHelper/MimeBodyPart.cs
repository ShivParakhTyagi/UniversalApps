using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json.Linq;

namespace MultipartHelperUWP.MultipartReaderHelper
{
    public class MimeBodyPart : IDisposable
    {
        private static readonly Type _streamType = typeof(Stream);
        private Stream _outputStream;
        private MultipartStreamProvider _streamProvider;
        private HttpContent _parentContent;
        private HttpContent _content;
        private HttpContentHeaders _headers;

        public MimeBodyPart(
            MultipartStreamProvider streamProvider,
            int maxBodyPartHeaderSize,
            HttpContent parentContent)
        {
            this._streamProvider = streamProvider;
            this._parentContent = parentContent;
            this.Segments = new List<ArraySegment<byte>>(2);
            this._headers = FormattingUtilities.CreateEmptyContentHeaders();
            this.HeaderParser = new InternetMessageFormatHeaderParser((HttpHeaders)this._headers, maxBodyPartHeaderSize, true);
        }

        public InternetMessageFormatHeaderParser HeaderParser { get; private set; }

        public HttpContent GetCompletedHttpContent()
        {
            if (this._content == null)
                return (HttpContent)null;
            this._headers.CopyTo(this._content.Headers);
            return this._content;
        }

        public List<ArraySegment<byte>> Segments { get; private set; }

        public bool IsComplete { get; set; }

        public bool IsFinal { get; set; }

        public async Task WriteSegment(
            ArraySegment<byte> segment,
            CancellationToken cancellationToken)
        {
            Stream stream = this.GetOutputStream();
            await stream.WriteAsync(segment.Array, segment.Offset, segment.Count, cancellationToken);
        }

        private Stream GetOutputStream()
        {
            if (this._outputStream == null)
            {
                try
                {
                    this._outputStream = this._streamProvider.GetStream(this._parentContent, this._headers);
                }
                catch (Exception ex)
                {
                    throw Error.InvalidOperation(ex, "Resources.ReadAsMimeMultipartStreamProviderException", (object)this._streamProvider.GetType().Name);
                }
                if (this._outputStream == null)
                    throw Error.InvalidOperation("Resources.ReadAsMimeMultipartStreamProviderNull", (object)this._streamProvider.GetType().Name, (object)MimeBodyPart._streamType.Name);
                if (!this._outputStream.CanWrite)
                    throw Error.InvalidOperation("Resources.ReadAsMimeMultipartStreamProviderReadOnly", (object)this._streamProvider.GetType().Name, (object)MimeBodyPart._streamType.Name);
                this._content = (HttpContent)new StreamContent(this._outputStream);
            }
            return this._outputStream;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize((object)this);
        }

        protected void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            this.CleanupOutputStream();
            this.CleanupHttpContent();
            this._parentContent = (HttpContent)null;
            this.HeaderParser = (InternetMessageFormatHeaderParser)null;
            this.Segments.Clear();
        }

        private void CleanupHttpContent()
        {
            if (!this.IsComplete && this._content != null)
                this._content.Dispose();
            this._content = (HttpContent)null;
        }

        private void CleanupOutputStream()
        {
            if (this._outputStream == null)
                return;
            MemoryStream outputStream = this._outputStream as MemoryStream;
            if (outputStream != null)
                outputStream.Position = 0L;
            else
                this._outputStream.Close();
            this._outputStream = (Stream)null;
        }
    }
    public static class FormattingUtilities
    {
        private static readonly string[] dateFormats = new string[15]
        {
      "ddd, d MMM yyyy H:m:s 'GMT'",
      "ddd, d MMM yyyy H:m:s",
      "d MMM yyyy H:m:s 'GMT'",
      "d MMM yyyy H:m:s",
      "ddd, d MMM yy H:m:s 'GMT'",
      "ddd, d MMM yy H:m:s",
      "d MMM yy H:m:s 'GMT'",
      "d MMM yy H:m:s",
      "dddd, d'-'MMM'-'yy H:m:s 'GMT'",
      "dddd, d'-'MMM'-'yy H:m:s",
      "ddd MMM d H:m:s yyyy",
      "ddd, d MMM yyyy H:m:s zzz",
      "ddd, d MMM yyyy H:m:s",
      "d MMM yyyy H:m:s zzz",
      "d MMM yyyy H:m:s"
        };
        public static readonly Type HttpRequestMessageType = typeof(HttpRequestMessage);
        public static readonly Type HttpResponseMessageType = typeof(HttpResponseMessage);
        public static readonly Type HttpContentType = typeof(HttpContent);
        //public static readonly Type DelegatingEnumerableGenericType = typeof(DelegatingEnumerable<>);
        //public static readonly Type EnumerableInterfaceGenericType = typeof(IEnumerable<>);
        //public static readonly Type QueryableInterfaceGenericType = typeof(IQueryable<>);
        //public static readonly XsdDataContractExporter XsdDataContractExporter = new XsdDataContractExporter();
        private const string NonTokenChars = "()<>@,;:\\\"/[]?={}";
        public const double Match = 1.0;
        public const double NoMatch = 0.0;
        public const int DefaultMaxDepth = 256;
        public const int DefaultMinDepth = 1;
        public const string HttpRequestedWithHeader = "x-requested-with";
        public const string HttpRequestedWithHeaderValue = "XMLHttpRequest";
        public const string HttpHostHeader = "Host";
        public const string HttpVersionToken = "HTTP";

        public static bool IsJTokenType(Type type)
        {
            return typeof(JToken).IsAssignableFrom(type);
        }

        public static HttpContentHeaders CreateEmptyContentHeaders()
        {
            HttpContent httpContent = (HttpContent)null;
            HttpContentHeaders httpContentHeaders = (HttpContentHeaders)null;
            try
            {
                httpContent = (HttpContent)new StringContent(string.Empty);
                httpContentHeaders = httpContent.Headers;
                httpContentHeaders.Clear();
            }
            finally
            {
                httpContent?.Dispose();
            }
            return httpContentHeaders;
        }

        public static XmlDictionaryReaderQuotas CreateDefaultReaderQuotas()
        {
            return new XmlDictionaryReaderQuotas()
            {
                MaxArrayLength = int.MaxValue,
                MaxBytesPerRead = int.MaxValue,
                MaxDepth = 256,
                MaxNameTableCharCount = int.MaxValue,
                MaxStringContentLength = int.MaxValue
            };
        }

        public static string UnquoteToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token) || !token.StartsWith("\"", StringComparison.Ordinal) || (!token.EndsWith("\"", StringComparison.Ordinal) || token.Length <= 1))
                return token;
            return token.Substring(1, token.Length - 2);
        }

        public static bool ValidateHeaderToken(string token)
        {
            if (token == null)
                return false;
            foreach (char ch in token)
            {
                if (ch < '!' || ch > '~' || "()<>@,;:\\\"/[]?={}".IndexOf(ch) != -1)
                    return false;
            }
            return true;
        }

        public static string DateToString(DateTimeOffset dateTime)
        {
            return dateTime.ToUniversalTime().ToString("r", (IFormatProvider)CultureInfo.InvariantCulture);
        }

        public static bool TryParseDate(string input, out DateTimeOffset result)
        {
            return DateTimeOffset.TryParseExact(input, FormattingUtilities.dateFormats, (IFormatProvider)DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal, out result);
        }

        public static bool TryParseInt32(string value, out int result)
        {
            return int.TryParse(value, NumberStyles.None, (IFormatProvider)NumberFormatInfo.InvariantInfo, out result);
        }
    }
    public static class HttpHeaderExtensions
    {
        public static void CopyTo(this HttpContentHeaders fromHeaders, HttpContentHeaders toHeaders)
        {
            foreach (KeyValuePair<string, IEnumerable<string>> fromHeader in (HttpHeaders)fromHeaders)
                toHeaders.TryAddWithoutValidation(fromHeader.Key, fromHeader.Value);
        }
    }

    public static class StreamExtensions
    {
        public static void Close(this Stream stream)
        {
            stream.Dispose();
        }
    }

}