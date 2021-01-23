using System.Security.Claims;
using Horse.Core;
using Horse.Protocols.Http;

namespace Horse.Mvc.Controllers
{
    /// <summary>
    /// Controller interface for Horse MVC
    /// </summary>
    public interface IController
    {
        /// <summary>
        /// HTTP Request
        /// </summary>
        HttpRequest Request { get; }

        /// <summary>
        /// HTTP Response
        /// </summary>
        HttpResponse Response { get; }

        /// <summary>
        /// Underlying HTTP Server object of Horse MVC
        /// </summary>
        IHorseServer Server { get; }

        /// <summary>
        /// Get Claims for user associated for executing request
        /// </summary>
        ClaimsPrincipal User { get; }
    }
}