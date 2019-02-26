using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace UwpHttpContentMultipartExtensions
{
    public static class HttpContentMultipartExtensions
    {

        /// <summary>Reads all body parts within a MIME multipart message and produces a set of <see cref="T:System.Net.Http.HttpContent" /> instances as a result using the streamProvider instance to determine where the contents of each body part is written.</summary>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> representing the tasks of getting the collection of <see cref="T:System.Net.Http.HttpContent" /> instances where each instance represents a body part.</returns>
        /// <param name="content">An existing <see cref="T:System.Net.Http.HttpContent" /> instance to use for the object's content.</param>
        /// <param name="streamProvider">A stream provider providing output streams for where to write body parts as they are parsed.</param>
        /// <typeparam name="T">The type of the MIME multipart.</typeparam>
        public static Task<T> ReadAsMultipartAsync<T>(this HttpContent content, T streamProvider) where T : MultipartStreamProvider
        {
            return content.ReadAsMultipartAsync<T>(streamProvider, 32768);
        }

        /// <summary>Reads all body parts within a MIME multipart message and produces a set of <see cref="T:System.Net.Http.HttpContent" /> instances as a result using the streamProvider instance to determine where the contents of each body part is written.</summary>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> representing the tasks of getting the collection of <see cref="T:System.Net.Http.HttpContent" /> instances where each instance represents a body part.</returns>
        /// <param name="content">An existing <see cref="T:System.Net.Http.HttpContent" /> instance to use for the object's content.</param>
        /// <param name="streamProvider">A stream provider providing output streams for where to write body parts as they are parsed.</param>
        /// <param name="cancellationToken">The token to cancel the operation.</param>
        /// <typeparam name="T">The type of the MIME multipart.</typeparam>
        public static Task<T> ReadAsMultipartAsync<T>(
          this HttpContent content,
          T streamProvider,
          CancellationToken cancellationToken)
          where T : MultipartStreamProvider
        {
            return content.ReadAsMultipartAsync<T>(streamProvider, 32768, cancellationToken);
        }

        /// <summary>Reads all body parts within a MIME multipart message and produces a set of <see cref="T:System.Net.Http.HttpContent" /> instances as a result using the streamProvider instance to determine where the contents of each body part is written and bufferSize as read buffer size.</summary>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> representing the tasks of getting the collection of <see cref="T:System.Net.Http.HttpContent" /> instances where each instance represents a body part.</returns>
        /// <param name="content">An existing <see cref="T:System.Net.Http.HttpContent" /> instance to use for the object's content.</param>
        /// <param name="streamProvider">A stream provider providing output streams for where to write body parts as they are parsed.</param>
        /// <param name="bufferSize">Size of the buffer used to read the contents.</param>
        /// <typeparam name="T">The type of the MIME multipart.</typeparam>
        public static Task<T> ReadAsMultipartAsync<T>(
          this HttpContent content,
          T streamProvider,
          int bufferSize)
          where T : MultipartStreamProvider
        {
            return content.ReadAsMultipartAsync<T>(streamProvider, bufferSize, CancellationToken.None);
        }
        /// <summary>Reads all body parts within a MIME multipart message and produces a set of <see cref="T:System.Net.Http.HttpContent" /> instances as a result using the streamProvider instance to determine where the contents of each body part is written and bufferSize as read buffer size.</summary>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> representing the tasks of getting the collection of <see cref="T:System.Net.Http.HttpContent" /> instances where each instance represents a body part.</returns>
        /// <param name="content">An existing <see cref="T:System.Net.Http.HttpContent" /> instance to use for the object's content.</param>
        /// <param name="streamProvider">A stream provider providing output streams for where to write body parts as they are parsed.</param>
        /// <param name="bufferSize">Size of the buffer used to read the contents.</param>
        /// <param name="cancellationToken">The token to cancel the operation.</param>
        /// <typeparam name="T">The type of the MIME multipart.</typeparam>
        public static async Task<T> ReadAsMultipartAsync<T>(
            this HttpContent content,
            T streamProvider,
            int bufferSize,
            CancellationToken cancellationToken)
            where T : MultipartStreamProvider
        {
            if (content == null)
                throw Error.ArgumentNull(nameof(content));
            if ((object) (T) streamProvider == null)
                throw Error.ArgumentNull(nameof(streamProvider));
            if (bufferSize < 256)
            {
                throw Error.ArgumentMustBeGreaterThanOrEqualTo(nameof(bufferSize), (object) bufferSize, (object) 256);
            }

            Stream stream;
            try
            {
                stream = await content.ReadAsStreamAsync();
            }
            catch (Exception ex)
            {
                throw new IOException("Resources.ReadAsMimeMultipartErrorReading {0}", ex);
                //throw new IOException(Resources.ReadAsMimeMultipartErrorReading, ex);
            }

            T obj;
            using (MimeMultipartBodyPartParser parser =
                new MimeMultipartBodyPartParser(content, (MultipartStreamProvider) streamProvider))
            {
                byte[] data = new byte[bufferSize];
                HttpContentMultipartExtensions.MultipartAsyncContext context =
                    new HttpContentMultipartExtensions.MultipartAsyncContext(stream, parser, data,
                        (ICollection<HttpContent>) streamProvider.Contents);
                await HttpContentMultipartExtensions.MultipartReadAsync(context, cancellationToken);
                await streamProvider.ExecutePostProcessingAsync(cancellationToken);
                obj = streamProvider;
            }

            return obj;
        }

        private static async Task MultipartReadAsync(
            HttpContentMultipartExtensions.MultipartAsyncContext context,
            CancellationToken cancellationToken)
        {
            label_1:
            int bytesRead;
            try
            {
                bytesRead = await context.ContentStream.ReadAsync(context.Data, 0, context.Data.Length, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new IOException("Resources.ReadAsMimeMultipartErrorReading", ex);
            }
            IEnumerable<MimeBodyPart> parts = context.MimeParser.ParseBuffer(context.Data, bytesRead);
            using (IEnumerator<MimeBodyPart> enumerator = parts.GetEnumerator())
            {
                MimeBodyPart part;
                do
                {
                    if (enumerator.MoveNext())
                    {
                        part = enumerator.Current;
                        foreach (ArraySegment<byte> segment in part.Segments)
                        {
                            try
                            {
                                await part.WriteSegment(segment, cancellationToken);
                            }
                            catch (Exception ex)
                            {
                                part.Dispose();
                                throw new IOException("Resources.ReadAsMimeMultipartErrorWriting", ex);
                            }
                        }
                    }
                    else
                        goto label_1;
                }
                while (!HttpContentMultipartExtensions.CheckIsFinalPart(part, context.Result));
            }
        }

        private static bool CheckIsFinalPart(MimeBodyPart part, ICollection<HttpContent> result)
        {
            if (!part.IsComplete)
                return false;
            HttpContent completedHttpContent = part.GetCompletedHttpContent();
            if (completedHttpContent != null)
                result.Add(completedHttpContent);
            bool isFinal = part.IsFinal;
            part.Dispose();
            return isFinal;
        }
        private class MultipartAsyncContext
        {
            public MultipartAsyncContext(
                Stream contentStream,
                MimeMultipartBodyPartParser mimeParser,
                byte[] data,
                ICollection<HttpContent> result)
            {
                this.ContentStream = contentStream;
                this.Result = result;
                this.MimeParser = mimeParser;
                this.Data = data;
            }

            public Stream ContentStream { get; private set; }

            public ICollection<HttpContent> Result { get; private set; }

            public byte[] Data { get; private set; }

            public MimeMultipartBodyPartParser MimeParser { get; private set; }
        }
    }
}