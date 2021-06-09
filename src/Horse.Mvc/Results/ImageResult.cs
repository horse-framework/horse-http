using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Horse.Core;

namespace Horse.Mvc.Results
{
    /// <summary>
    /// Image result such as JPG, PNG, ICO, SVG
    /// </summary>
    public class ImageResult : IActionResult
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
        public ImageResult(string filename, byte[] data)
        {
            Code = HttpStatusCode.OK;
            int li = filename.LastIndexOf('.');
            if (li < 0)
                ContentType = ContentTypes.OCTET_STREAM;
            else
            {
                string extension = filename.Substring(li + 1).ToLowerInvariant();
                if (extension == "jpg")
                    extension = "jpeg";
                else if (extension == "svg")
                    extension += "+xml";
                
                ContentType = $"image/{extension}";
            }

            Headers = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            Stream = new MemoryStream(data);
        }
    }
}