using System;
using System.Net;
using System.Threading.Tasks;
using Horse.Mvc.Results;
using Horse.Protocols.Http;

namespace Horse.Mvc.Errors
{
    /// <summary>
    /// Default Error Handler.
    /// If there is no error handler in the project
    /// to disable development error handler in production mode
    /// this handler will be activated
    /// </summary>
    public class DefaultErrorHandler : IErrorHandler
    {
        /// <summary>
        /// Writes a short 500 - Internal Server Error to the response.
        /// Hides exception information
        /// </summary>
        public Task<IActionResult> Error(HttpRequest request, Exception ex)
        {
            IActionResult error = new HtmlResult(PredefinedResults.Statuses[HttpStatusCode.InternalServerError]);
            return Task.FromResult(error);
        }
    }
}