using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using Horse.Core;
using Horse.Mvc.Auth;
using Horse.Mvc.Controllers;
using Horse.Mvc.Errors;
using Horse.Mvc.Middlewares;
using Horse.Mvc.Results;
using Horse.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("Test.Mvc")]

namespace Horse.Mvc
{
    /// <summary>
    /// Horse Facade object of Horse HTTP Server
    /// </summary>
    public class HorseMvc
    {
        #region Properties

        /// <summary>
        /// All routes. This list is prepared with Init method.
        /// Loads all types in assembly implemented from IController and their actions with Http Method Attribute.
        /// </summary>
        internal List<RouteLeaf> Routes { get; private set; }

        /// <summary>
        /// Action Routes
        /// </summary>
        internal List<ActionRoute> ActionRoutes { get; private set; }

        /// <summary>
        /// File download routes
        /// </summary>
        public List<FileRoute> FileRoutes { get; private set; }

        /// <summary>
        /// Server objects of the MVC
        /// </summary>
        public IHorseServer Server { get; set; }

        /// <summary>
        /// Default HTTP status results
        /// </summary>
        public Dictionary<HttpStatusCode, IActionResult> StatusCodeResults { get; } = new Dictionary<HttpStatusCode, IActionResult>();

        /// <summary>
        /// Microsoft Dependency Injection Service Provider
        /// </summary>
        public IServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// Horse MVC Route finder. For every HTTP Request, this finder matches the request Path with Routes list.
        /// </summary>
        public IRouteFinder RouteFinder { get; set; }

        /// <summary>
        /// Horse MVC Controller Factory. For every Request, when the controller if found for the requested path,
        /// ControllerFactory creates the controller object and fills all properties and Dependency Injection parameters.
        /// </summary>
        public IControllerFactory ControllerFactory { get; set; }

        /// <summary>
        /// Reads User information from the request and creates ClaimsPrincipal class
        /// </summary>
        public IClaimsPrincipalValidator ClaimsPrincipalValidator { get; set; }

        /// <summary>
        /// Pre-defined Policy container
        /// </summary>
        public IPolicyContainer Policies { get; set; }

        /// <summary>
        /// Non-Development Mode Error Handler object
        /// </summary>
        public IErrorHandler ErrorHandler { get; set; }

        /// <summary>
        /// Used for 404 Results. As default, its 404 StatusCodeResult.
        /// In order to customize, development can change this property.
        /// </summary>
        public IActionResult NotFoundResult { get; set; }

        /// <summary>
        /// Development mode for the Horse Server.
        /// It's loaded from ServerOptions (usually from horse.json file)
        /// Can be changed progammatically
        /// </summary>
        public bool IsDevelopment { get; set; }

        /// <summary>
        /// Mvc application builder
        /// </summary>
        internal MvcAppBuilder AppBuilder { get; }

        /// <summary>
        /// Default JSON serialization options for input models.
        /// Used for deserializing action method parameters
        /// </summary>
        public JsonSerializationOptions JsonInputOptions { get; } = new JsonSerializationOptions();

        /// <summary>
        /// Default JSON serialization options for output models.
        /// Used for serializing action method return values
        /// </summary>
        public JsonSerializationOptions JsonOutputOptions { get; } = new JsonSerializationOptions();

        private readonly IServiceCollection _services;

        #endregion

        #region Constructors - Destructors

        /// <summary>
        /// Creates new Horse MVC
        /// </summary>
        public HorseMvc() : this(new ServiceCollection())
        {
        }

        /// <summary>
        /// Creates new Horse MVC
        /// </summary>
        public HorseMvc(IServiceCollection services)
        {
            Routes = new List<RouteLeaf>();
            RouteFinder = new RouteFinder();
            ControllerFactory = new ControllerFactory();
            NotFoundResult = StatusCodeResult.NotFound();
            ErrorHandler = new DefaultErrorHandler();
            Policies = new PolicyContainer();

            AppBuilder = new MvcAppBuilder(this);
            _services = services;
        }

        #endregion

        #region Init

        /// <summary>
        /// Inits Horse MVC
        /// </summary>
        public void Init(Action<IServiceCollection> action)
        {
            Init();
            action(_services);
        }

        /// <summary>
        /// Inits Horse MVC
        /// </summary>
        public void Init()
        {
            Routes = new List<RouteLeaf>();
            FileRoutes = new List<FileRoute>();
            ActionRoutes = new List<ActionRoute>();
            CreateRoutes();
        }

        /// <summary>
        /// Loads all IController types from the assembly and searches route info in all types.
        /// </summary>
        public void CreateRoutes(Assembly assembly = null)
        {
            RouteBuilder builder = new RouteBuilder();

            Type interfaceType = typeof(IController);

            List<Assembly> assemblies = new List<Assembly>();
            if (assembly == null)
            {
                Assembly entryAssembly = Assembly.GetEntryAssembly();
                if (entryAssembly == null)
                    throw new ArgumentNullException("Entry Assembly could not be found");

                assemblies.Add(entryAssembly);
                assemblies.AddRange(entryAssembly.GetReferencedAssemblies().Select(Assembly.Load));
            }
            else
                assemblies.Add(assembly);

            List<Type> types = assemblies
                .SelectMany(x => x.GetTypes())
                .Where(type => interfaceType.IsAssignableFrom(type))
                .ToList();

            List<RouteLeaf> leaves = new List<RouteLeaf>();

            foreach (Type type in types)
            {
                if (type.IsInterface)
                    continue;

                if (type.IsAssignableFrom(typeof(HorseController)) && typeof(HorseController).IsAssignableFrom(type))
                    continue;

                leaves.AddRange(builder.BuildRoutes(type));
                _services.AddScoped(type);
            }

            foreach (RouteLeaf root in leaves)
                builder.SortChildren(root);

            builder.SortRoutes(leaves);
            foreach (RouteLeaf route in leaves)
                Routes.Add(route);
        }

        /// <summary>
        /// Loads all IController types from the assembly
        /// </summary>
        public void ImportAssembly(Assembly assembly)
        {
            CreateRoutes(assembly);
        }

        #endregion

        #region Run

        /// <summary>
        /// Runs Horse MVC Server as async, with middleware implementation
        /// </summary>
        public void Use(IServiceProvider provider, Action<IMvcAppBuilder> action)
        {
            ServiceProvider = provider;
            if (action != null)
                action(AppBuilder);
        }

        /// <summary>
        /// Runs Horse MVC Server as async without middleware implementation
        /// </summary>
        public void Use(IServiceProvider provider)
        {
            ServiceProvider = provider;
        }

        /// <summary>
        /// Runs Horse MVC Server as async, with middleware implementation
        /// </summary>
        public void Use(Action<IMvcAppBuilder> action)
        {
            ServiceProvider = _services.BuildServiceProvider();
            action?.Invoke(AppBuilder);
        }

        /// <summary>
        /// Runs Horse MVC Server as async without middleware implementation
        /// </summary>
        public void Use()
        {
            ServiceProvider = _services.BuildServiceProvider();
        }

        #endregion
    }
}