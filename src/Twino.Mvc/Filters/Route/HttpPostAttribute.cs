﻿using System;

namespace Twino.Mvc.Filters.Route
{
    /// <summary>
    /// Attribute for action methods routed with HTTP POST method
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class HttpPostAttribute : HttpMethodAttribute
    {
        /// <summary>
        /// Creates new HTTP Method POST attribute
        /// </summary>
        public HttpPostAttribute() : this(null)
        {
        }

        /// <summary>
        /// Creates new HTTP Method POST attribute with specified route pattern
        /// </summary>
        public HttpPostAttribute(string pattern) : base("POST", pattern)
        {
        }
    }
}