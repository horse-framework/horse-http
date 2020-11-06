using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Twino.Protocols.Http;

namespace Twino.Mvc.Controllers
{
    /// <summary>
    /// Factory object for the controller.
    /// For each request, this objects creates a controller from the specified parameters
    /// </summary>
    public class ControllerFactory : IControllerFactory
    {
        /// <summary>
        /// Creates new instance of a TwinoController object
        /// </summary>
        public TwinoController CreateInstance(TwinoMvc mvc, Type controllerType, HttpRequest request, HttpResponse response, IServiceScope scope)
        {
            object controller = scope.ServiceProvider.GetService(controllerType);

            //if the application comes here, we are sure all parameters are created
            //now we can create instance with these parameter values
            TwinoController result = (TwinoController) controller;
            if (result == null)
                throw new InvalidOperationException($"Can't resolve {controllerType.FullName} controller type.");

            //set the controller properties
            result.Request = request;
            result.Response = response;
            result.Server = mvc.Server;
            result.CurrentScope = scope;
            result.Mvc = mvc;

            return result;
        }
    }
}