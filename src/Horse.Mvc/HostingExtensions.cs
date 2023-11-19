using System;
using System.Collections.Generic;
using Horse.Core;
using Horse.Mvc.Middlewares;
using Horse.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using HostOptions = Horse.Server.HostOptions;

namespace Horse.Mvc;

/// <summary>
/// Extension methods for Microsoft Extensions Hosting library
/// </summary>
public static class HostingExtensions
{
    /// <summary>
    /// Creates new Horse Server and Implements MVC with default options
    /// </summary>
    /// <param name="builder">Microsoft Extensions Host Builder</param>
    /// <param name="port">Server port</param>
    /// <returns></returns>
    public static IHostBuilder UseHorseMvc(this IHostBuilder builder, int port)
    {
        HorseServer server = new HorseServer();
        server.Options.Hosts = new List<HostOptions>();
        server.Options.Hosts.Add(new HostOptions
        {
            Port = port,
            SslEnabled = false
        });

        return UseHorseMvc(builder, server, null, null);
    }

    /// <summary>
    /// Creates new Horse Server and Implements MVC with intiailization configurations
    /// </summary>
    /// <param name="builder">Microsoft Extensions Host Builder</param>
    /// <param name="port">Server port</param>
    /// <param name="configure">Horse Mvc initialization action</param>
    /// <returns></returns>
    public static IHostBuilder UseHorseMvc(this IHostBuilder builder, int port, Action<HorseMvc> configure)
    {
        HorseServer server = new HorseServer();
        server.Options.Hosts = new List<HostOptions>();
        server.Options.Hosts.Add(new HostOptions
        {
            Port = port,
            SslEnabled = false
        });
        return UseHorseMvc(builder, new HorseServer(), configure, null);
    }

    /// <summary>
    /// Creates new Horse Server and Implements MVC with runner configurations
    /// </summary>
    /// <param name="builder">Microsoft Extensions Host Builder</param>
    /// <param name="port">Server port</param>
    /// <param name="app">Horse Mvc runner action</param>
    /// <returns></returns>
    public static IHostBuilder UseHorseMvc(this IHostBuilder builder, int port, Action<IMvcAppBuilder> app)
    {
        HorseServer server = new HorseServer();
        server.Options.Hosts = new List<HostOptions>();
        server.Options.Hosts.Add(new HostOptions
        {
            Port = port,
            SslEnabled = false
        });

        return UseHorseMvc(builder, server, null, app);
    }

    /// <summary>
    /// Creates new Horse Server and starts new Horse Mvc on specified Horse Server with all configurations
    /// </summary>
    /// <param name="builder">Microsoft Extensions Host Builder</param>
    /// <param name="port">Server port</param>
    /// <param name="configure">Horse Mvc initialization action</param>
    /// <param name="app">Horse Mvc runner action</param>
    /// <returns></returns>
    public static IHostBuilder UseHorseMvc(this IHostBuilder builder, int port, Action<HorseMvc> configure, Action<IMvcAppBuilder> app)
    {
        HorseServer server = new HorseServer();
        server.Options.Hosts = new List<HostOptions>();
        server.Options.Hosts.Add(new HostOptions
        {
            Port = port,
            SslEnabled = false
        });

        return UseHorseMvc(builder, server, configure, app);
    }

    /// <summary>
    /// Starts new Horse Mvc on specified Horse Server with runner configurations
    /// </summary>
    /// <param name="builder">Microsoft Extensions Host Builder</param>
    /// <param name="server">Horse Server</param>
    /// <param name="app">Horse Mvc runner action</param>
    /// <returns></returns>
    public static IHostBuilder UseHorseMvc(this IHostBuilder builder, HorseServer server, Action<IMvcAppBuilder> app)
    {
        return UseHorseMvc(builder, server, null, app);
    }

    /// <summary>
    /// Starts new Horse Mvc on specified Horse Server with initialization configurations
    /// </summary>
    /// <param name="builder">Microsoft Extensions Host Builder</param>
    /// <param name="server">Horse Server</param>
    /// <param name="configure">Horse Mvc initialization action</param>
    /// <returns></returns>
    public static IHostBuilder UseHorseMvc(this IHostBuilder builder, HorseServer server, Action<HorseMvc> configure)
    {
        return UseHorseMvc(builder, server, configure, null);
    }

    /// <summary>
    /// Starts new Horse Mvc on specified Horse Server with all configurations
    /// </summary>
    /// <param name="builder">Microsoft Extensions Host Builder</param>
    /// <param name="server">Horse Server</param>
    /// <param name="configure">Horse Mvc initialization action</param>
    /// <param name="app">Horse Mvc runner action</param>
    /// <returns></returns>
    public static IHostBuilder UseHorseMvc(this IHostBuilder builder, HorseServer server, Action<HorseMvc> configure, Action<IMvcAppBuilder> app)
    {
        builder.ConfigureServices((context, services) =>
        {
            HorseMvc mvc = new HorseMvc(services);
            configure?.Invoke(mvc);

            mvc.Init();

            services.AddSingleton<IHorseServer>(server);
            services.AddSingleton(server);
            services.AddSingleton(mvc);

            services.AddHostedService(provider =>
            {
                HorseMvcRunnerService hostedService = new HorseMvcRunnerService(server, mvc, provider, app);
                return hostedService;
            });
        });

        return builder;
    }
}