﻿using System;
using System.Threading.Tasks;
using Twino.Ioc;
using Twino.Protocols.Http;

namespace Twino.Mvc.Controllers
{
    /// <summary>
    /// Factory object for the controller.
    /// For each request, this objects creates a controller from the specified parameters
    /// </summary>
    public interface IControllerFactory
    {

        /// <summary>
        /// Creates new instance of a TwinoController object
        /// </summary>
        Task<TwinoController> CreateInstance(TwinoMvc mvc, Type controllerType, HttpRequest request, HttpResponse response, IContainerScope scope);

    }
}
