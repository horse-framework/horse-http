using System.Text.Json;
using Newtonsoft.Json;

namespace Horse.Mvc
{
    /// <summary>
    /// Serialization options
    /// </summary>
    public class JsonSerializationOptions
    {
        /// <summary>
        /// If true, newtonsoft library is used.
        /// Default is true.
        /// </summary>
        public bool UseNewtonsoft { get; set; } = true;

        /// <summary>
        /// System Text Options
        /// </summary>
        public JsonSerializerOptions SystemTextOptions { get; set; }

        /// <summary>
        /// Newtonsoft Options
        /// </summary>
        public JsonSerializerSettings NewtonsoftOptions { get; set; }
    }
}