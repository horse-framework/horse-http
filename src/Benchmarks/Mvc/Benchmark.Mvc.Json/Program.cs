using Horse.Mvc;
using Horse.Protocols.Http;
using Horse.Server;

namespace Benchmark.Mvc.Json
{
    class Program
    {
        static void Main(string[] args)
        {
            HorseMvc mvc = new HorseMvc();
            HorseServer
                server = new HorseServer();
            mvc.Init();
            server.UseMvc(mvc, HttpOptions.CreateDefault());
            server.Run();
        }
    }
}