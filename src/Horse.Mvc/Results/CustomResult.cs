using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Horse.Mvc.Results
{
    /// <summary>
    /// Custom Action result
    /// </summary>
    public class CustomResult : IActionResult
    {
        /// <summary>
        /// Result HTTP Status code
        /// </summary>
        public HttpStatusCode Code { get; set; }

        /// <summary>
        /// Result content type (such as application/json, text/xml, text/plain)
        /// </summary>
        public string ContentType { get; }

        /// <summary>
        /// Result content body
        /// </summary>
        public Stream Stream { get; }

        /// <summary>
        /// Additional custom headers with key and value
        /// </summary>
        public Dictionary<string, string> Headers { get; }

        /// <summary>
        /// Creates new Custom Result
        /// </summary>
        public CustomResult(HttpStatusCode statusCode, string contentType, byte[] data)
        {
            Code = statusCode;
            ContentType = contentType;
            Headers = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            Stream = new MemoryStream(data);
        }
    }
}