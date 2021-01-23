using System;
using System.Net;
using System.Threading.Tasks;
using Horse.Protocols.Http;
using Horse.Server;

namespace Benchmark.PlainText
{
    class Program
    {
        static void Main(string[] args)
        {
            HorseServer server = new HorseServer(ServerOptions.CreateDefault());
            server.UseHttp((request, response) =>
            {
                if (request.Path.Equals("/plaintext", StringComparison.InvariantCultureIgnoreCase))
                {
                    response.SetToText();
                    return response.WriteAsync("Hello, World!");
                }

                response.StatusCode = HttpStatusCode.NotFound;
                return Task.CompletedTask;
                
            }, HttpOptions.CreateDefault());

            server.Run(5000);
        }
    }
}