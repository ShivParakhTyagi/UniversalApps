using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;

namespace UwpHttpContentMultipartExtensions
{
    /// <summary>Represents a multipart memory stream provider.</summary>
    public class MultipartMemoryStreamProvider : MultipartStreamProvider
    {
        /// <summary>Returns the <see cref="T:System.IO.Stream" /> for the <see cref="T:System.Net.Http.MultipartMemoryStreamProvider" />.</summary>
        /// <returns>The <see cref="T:System.IO.Stream" /> for the <see cref="T:System.Net.Http.MultipartMemoryStreamProvider" />.</returns>
        /// <param name="parent">A <see cref="T:System.Net.Http.HttpContent" /> object.</param>
        /// <param name="headers">The HTTP content headers.</param>
        public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
        {
            if (parent == null)
                throw Error.ArgumentNull(nameof(parent));
            if (headers == null)
                throw Error.ArgumentNull(nameof(headers));
            return (Stream)new MemoryStream();
        }
    }
}