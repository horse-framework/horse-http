using System;
using System.Threading.Tasks;
using Horse.Protocols.Http;

namespace Horse.Mvc.Errors
{
    /// <summary>
    /// Each Horse MVC Request is executed in try/catch block and Horse catches the exception if thrown.
    /// If you want to catch these exceptions, you can implement this interface and create new class,
    /// and set HorseMvc.ErrorHandler property.
    /// </summary>
    public interface IErrorHandler
    {
        /// <summary>
        /// [async] When an uncatched error has occured in HTTP Request, this method will be called.
        /// </summary>
        Task<IActionResult> Error(HttpRequest request, Exception ex);
    }
}