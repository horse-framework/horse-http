using System;
using System.Net;
using Horse.Protocols.Http;
using Horse.Server;

namespace Sample.Http.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            HorseServer server = new HorseServer(ServerOptions.CreateDefault());
            server.UseHttp(async (request, response) =>
            {
                if (request.Path.Equals("/plaintext", StringComparison.InvariantCultureIgnoreCase))
                {
                    response.SetToText();
                    await response.WriteAsync("Hello, World!");
                }
                else
                    response.StatusCode = HttpStatusCode.NotFound;
            });

            server.Run(22);
        }
    }
}