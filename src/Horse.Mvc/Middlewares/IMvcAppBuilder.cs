using System;
using System.Net;
using System.Threading.Tasks;
using Horse.Protocols.Http;

namespace Horse.Mvc.Middlewares
{
    /// <summary>
    /// Application builder for horse MVC
    /// </summary>
    public interface IMvcAppBuilder
    {
        /// <summary>
        /// Gets MSDI service provider for horse mvc
        /// </summary>
        IServiceProvider GetProvider();

        /// <summary>
        /// Uses middleware from instance
        /// </summary>
        void UseMiddleware(IMiddleware middleware);

        /// <summary>
        /// Creates new instance of type and uses it as middleware
        /// </summary>
        void UseMiddleware<TMiddleware>() where TMiddleware : IMiddleware;

        /// <summary>
        /// Uses static files in specific physical path
        /// </summary>
        void UseFiles(string urlPath, string physicalPath);

        /// <summary>
        /// Uses static files in multiple physical paths.
        /// Searches requested files in index order.
        /// </summary>
        void UseFiles(string urlPath, string[] physicalPaths);

        /// <summary>
        /// Uses static files in specific physical path.
        /// Each request is filtered by validation method
        /// </summary>
        void UseFiles(string urlPath, string physicalPath, Func<HttpRequest, HttpStatusCode> validation);

        /// <summary>
        /// Uses static files in multiple physical paths.
        /// Searches requested files in index order.
        /// Each request is filtered by validation method
        /// </summary>
        void UseFiles(string urlPath, string[] physicalPaths, Func<HttpRequest, HttpStatusCode> validation);

        /// <summary>
        /// Uses action route
        /// </summary>
        void UseActionRoute(string urlPath, Func<HttpRequest, Task<IActionResult>> action);

        /// <summary>
        /// Uses action route
        /// </summary>
        void UseActionRoute(string urlPath, Func<HttpRequest, IActionResult> action);
    }
}