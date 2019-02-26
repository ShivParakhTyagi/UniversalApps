using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;

namespace MultipartHelperUWP.MultipartReaderHelper
{
    public class MimeMultipartBodyPartParser : IDisposable
    {
        private ArraySegment<byte>[] _parsedBodyPart = new ArraySegment<byte>[2];
        private bool _isFirst = true;
        internal const long DefaultMaxMessageSize = 9223372036854775807;
        private const int DefaultMaxBodyPartHeaderSize = 4096;
        private MimeMultipartParser _mimeParser;
        private MimeMultipartParser.State _mimeStatus;
        private MimeBodyPart _currentBodyPart;
        private ParserState _bodyPartHeaderStatus;
        private int _maxBodyPartHeaderSize;
        private MultipartStreamProvider _streamProvider;
        private HttpContent _content;

        public MimeMultipartBodyPartParser(HttpContent content, MultipartStreamProvider streamProvider)
            : this(content, streamProvider, long.MaxValue, 4096)
        {
        }

        public MimeMultipartBodyPartParser(
            HttpContent content,
            MultipartStreamProvider streamProvider,
            long maxMessageSize,
            int maxBodyPartHeaderSize)
        {
            this._mimeParser =
                new MimeMultipartParser(MimeMultipartBodyPartParser.ValidateArguments(content, maxMessageSize, true),
                    maxMessageSize);
            this._currentBodyPart = new MimeBodyPart(streamProvider, maxBodyPartHeaderSize, content);
            this._content = content;
            this._maxBodyPartHeaderSize = maxBodyPartHeaderSize;
            this._streamProvider = streamProvider;
        }

        public static bool IsMimeMultipartContent(HttpContent content)
        {
            try
            {
                return MimeMultipartBodyPartParser.ValidateArguments(content, long.MaxValue, false) != null;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize((object) this);
        }

        public IEnumerable<MimeBodyPart> ParseBuffer(byte[] data, int bytesRead)
        {
            int bytesConsumed = 0;
            bool isFinal = false;
            if (bytesRead == 0 && !this._mimeParser.IsWaitingForEndOfMessage)
            {
                this.CleanupCurrentBodyPart();
                throw new IOException("ReadAsMimeMultipartUnexpectedTermination");
            }

            this._currentBodyPart.Segments.Clear();
            while (this._mimeParser.CanParseMore(bytesRead, bytesConsumed))
            {
                this._mimeStatus = this._mimeParser.ParseBuffer(data, bytesRead, ref bytesConsumed,
                    out this._parsedBodyPart[0], out this._parsedBodyPart[1], out isFinal);
                if (this._mimeStatus != MimeMultipartParser.State.BodyPartCompleted &&
                    this._mimeStatus != MimeMultipartParser.State.NeedMoreData)
                {
                    this.CleanupCurrentBodyPart();
                    throw Error.InvalidOperation("ReadAsMimeMultipartParseError", (object) bytesConsumed,
                        (object) data);
                }

                if (this._isFirst)
                {
                    if (this._mimeStatus == MimeMultipartParser.State.BodyPartCompleted)
                        this._isFirst = false;
                }
                else
                {
                    foreach (ArraySegment<byte> arraySegment in this._parsedBodyPart)
                    {
                        if (arraySegment.Count != 0)
                        {
                            if (this._bodyPartHeaderStatus != ParserState.Done)
                            {
                                int offset = arraySegment.Offset;
                                this._bodyPartHeaderStatus =
                                    this._currentBodyPart.HeaderParser.ParseBuffer(arraySegment.Array,
                                        arraySegment.Count + arraySegment.Offset, ref offset);
                                if (this._bodyPartHeaderStatus == ParserState.Done)
                                    this._currentBodyPart.Segments.Add(new ArraySegment<byte>(arraySegment.Array,
                                        offset, arraySegment.Count + arraySegment.Offset - offset));
                                else if (this._bodyPartHeaderStatus != ParserState.NeedMoreData)
                                {
                                    this.CleanupCurrentBodyPart();
                                    throw Error.InvalidOperation("ReadAsMimeMultipartHeaderParseError", (object) offset,
                                        (object) arraySegment.Array);
                                }
                            }
                            else
                                this._currentBodyPart.Segments.Add(arraySegment);
                        }
                    }

                    if (this._mimeStatus == MimeMultipartParser.State.BodyPartCompleted)
                    {
                        MimeBodyPart completed = this._currentBodyPart;
                        completed.IsComplete = true;
                        completed.IsFinal = isFinal;
                        this._currentBodyPart = new MimeBodyPart(this._streamProvider, this._maxBodyPartHeaderSize,
                            this._content);
                        this._mimeStatus = MimeMultipartParser.State.NeedMoreData;
                        this._bodyPartHeaderStatus = ParserState.NeedMoreData;
                        yield return completed;
                    }
                    else
                        yield return this._currentBodyPart;
                }
            }
        }

        protected void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            this._mimeParser = (MimeMultipartParser) null;
            this.CleanupCurrentBodyPart();
        }

        private static string ValidateArguments(
            HttpContent content,
            long maxMessageSize,
            bool throwOnError)
        {
            if (maxMessageSize < 10L)
            {
                if (throwOnError)
                    throw Error.ArgumentMustBeGreaterThanOrEqualTo(nameof(maxMessageSize), (object) maxMessageSize,
                        (object) 10);
                return (string) null;
            }

            MediaTypeHeaderValue contentType = content.Headers.ContentType;
            if (contentType == null)
            {
                if (throwOnError)
                    throw Error.Argument(nameof(content), "ReadAsMimeMultipartArgumentNoContentType",
                        (object) typeof(HttpContent).Name, (object) "multipart/");
                return (string) null;
            }

            if (!contentType.MediaType.StartsWith("multipart", StringComparison.OrdinalIgnoreCase))
            {
                if (throwOnError)
                    throw Error.Argument(nameof(content), "ReadAsMimeMultipartArgumentNoMultipart",
                        (object) typeof(HttpContent).Name, (object) "multipart/");
                return (string) null;
            }

            string str = (string) null;
            foreach (NameValueHeaderValue parameter in (IEnumerable<NameValueHeaderValue>) contentType.Parameters)
            {
                if (parameter.Name.Equals("boundary", StringComparison.OrdinalIgnoreCase))
                {
                    str = FormattingUtilities.UnquoteToken(parameter.Value);
                    break;
                }
            }

            if (str != null)
                return str;
            if (throwOnError)
                throw Error.Argument(nameof(content), "ReadAsMimeMultipartArgumentNoBoundary",
                    (object) typeof(HttpContent).Name, (object) "multipart", (object) "boundary");
            return (string) null;
        }

        private void CleanupCurrentBodyPart()
        {
            if (this._currentBodyPart == null)
                return;
            this._currentBodyPart.Dispose();
            this._currentBodyPart = (MimeBodyPart) null;
        }
    }

    public enum ParserState
    {
        NeedMoreData,
        Done,
        Invalid,
        DataTooBig,
    }
}