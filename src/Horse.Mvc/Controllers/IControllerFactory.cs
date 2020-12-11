using System;
using Microsoft.Extensions.DependencyInjection;
using Horse.Protocols.Http;

namespace Horse.Mvc.Controllers
{
    /// <summary>
    /// Factory object for the controller.
    /// For each request, this objects creates a controller from the specified parameters
    /// </summary>
    public interface IControllerFactory
    {
        /// <summary>
        /// Creates new instance of a HorseController object
        /// </summary>
        HorseController CreateInstance(HorseMvc mvc, Type controllerType, HttpRequest request, HttpResponse response, IServiceScope scope);
    }
}