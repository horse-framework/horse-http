using System;
using System.Threading.Tasks;
using Horse.Protocols.Http;

namespace Horse.Mvc.Routing
{
    /// <summary>
    /// Action route definition
    /// </summary>
    public class ActionRoute
    {
        /// <summary>
        /// Virtual path
        /// </summary>
        public string VirtualPath { get; }

        /// <summary>
        /// For async actions, async action method
        /// </summary>
        public Func<HttpRequest, Task<IActionResult>> AsyncAction { get; }

        /// <summary>
        /// For non async actions, action method
        /// </summary>
        public Func<HttpRequest, IActionResult> Action { get; }

        /// <summary>
        /// Creates new action route with async action method
        /// </summary>
        public ActionRoute(string virtualPath, Func<HttpRequest, Task<IActionResult>> asyncAction)
        {
            VirtualPath = virtualPath;
            AsyncAction = asyncAction;
            Action = null;
        }

        /// <summary>
        /// Creates new action route with non action method
        /// </summary>
        public ActionRoute(string virtualPath, Func<HttpRequest, IActionResult> action)
        {
            VirtualPath = virtualPath;
            Action = action;
            AsyncAction = null;
        }
    }
}