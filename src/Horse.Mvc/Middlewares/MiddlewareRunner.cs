using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Horse.Protocols.Http;

namespace Horse.Mvc.Middlewares
{
    /// <summary>
    /// Middleware runner object.
    /// Created for per request organizes and executes sequence all middlewares for the request
    /// </summary>
    internal class MiddlewareRunner
    {
        /// <summary>
        /// Last middleware's result object
        /// </summary>
        internal IActionResult LastResult { get; private set; }

        private readonly IServiceScope _scope;

        public MiddlewareRunner(IServiceScope scope)
        {
            _scope = scope;
        }

        private void SetResult(IActionResult result = null)
        {
            LastResult = result;
        }

        /// <summary>
        /// Executes all middlewares
        /// </summary>
        internal async Task RunSequence(MvcAppBuilder app, HttpRequest request, HttpResponse response)
        {
            foreach (MiddlewareDescriptor descriptor in app.Descriptors)
            {
                IMiddleware middleware = CreateInstance(descriptor);
                await middleware.Invoke(request, response, SetResult);

                if (LastResult != null)
                    break;
            }
        }

        /// <summary>
        /// Creates middleware object from descriptor
        /// </summary>
        private IMiddleware CreateInstance(MiddlewareDescriptor descriptor)
        {
            if (descriptor.Instance != null)
                return descriptor.Instance;

            if (descriptor.ConstructorParameters.Length == 0)
                return (IMiddleware) Activator.CreateInstance(descriptor.MiddlewareType);

            object[] parameters = new object[descriptor.ConstructorParameters.Length];

            for (int i = 0; i < descriptor.ConstructorParameters.Length; i++)
            {
                Type type = descriptor.ConstructorParameters[i];

                if (typeof(IServiceScope).IsAssignableFrom(type))
                    parameters[i] = _scope;
                else
                    parameters[i] = _scope.ServiceProvider.GetService(type);
            }

            return (IMiddleware) Activator.CreateInstance(descriptor.MiddlewareType, parameters);
        }
    }
}