using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Horse.Mvc.Results
{
    /// <summary>
    /// CSS Action Result
    /// </summary>
    public class CssResult : IActionResult
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
        /// Creates new CSS Result from HTML string
        /// </summary>
        public CssResult(string content)
        {
            Code = HttpStatusCode.OK;
            ContentType = "text/css";
            Headers = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            Stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        }

        /// <summary>
        /// Creates new CSS result from byte array
        /// </summary>
        public CssResult(byte[] bytes)
        {
            Code = HttpStatusCode.OK;
            ContentType = "text/css";
            Headers = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            Stream = new MemoryStream(bytes);
        }
    }
}