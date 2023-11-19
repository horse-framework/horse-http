using System;
using System.Threading;
using System.Threading.Tasks;
using Horse.Mvc.Middlewares;
using Horse.Server;
using Microsoft.Extensions.Hosting;

namespace Horse.Mvc;

internal class HorseMvcRunnerService : IHostedService
{
    private readonly HorseServer _server;
    private readonly HorseMvc _mvc;
    private readonly IServiceProvider _provider;
    private readonly Action<IMvcAppBuilder> _runner;

    public HorseMvcRunnerService(HorseServer server, HorseMvc mvc, IServiceProvider provider, Action<IMvcAppBuilder> runner)
    {
        _server = server;
        _mvc = mvc;
        _provider = provider;
        _runner = runner;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_runner != null)
            _mvc.Use(_provider, _runner);
        else
            _mvc.Use(_provider);

        _server.UseMvc(_mvc);

        if (!_server.IsRunning)
            _server.Start();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (_server.IsRunning)
            _server.Stop();
        
        return Task.CompletedTask;
    }
}