using System;
using System.Net;
using System.Threading.Tasks;
using Horse.Protocols.Http;
using Horse.Server;

namespace Benchmark.Json
{
    class Program
    {
        static void Main(string[] args)
        {
            HorseServer server = new HorseServer(ServerOptions.CreateDefault());
            server.UseHttp(async (request, response) =>
            {
                if (request.Path.Equals("/json", StringComparison.InvariantCultureIgnoreCase))
                    response.SetToJson(new {message = "Hello, World!"});
                else
                    response.StatusCode = HttpStatusCode.NotFound;

                await Task.CompletedTask;
            }, HttpOptions.CreateDefault());

            server.Run();
        }
    }
}