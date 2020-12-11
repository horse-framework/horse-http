using System;
using Horse.Mvc;
using Horse.Server;

namespace Benchmark.Mvc.PlainText
{
    class Program
    {
        static void Main(string[] args)
        {
            HorseServer server = new HorseServer();

            HorseMvc mvc = new HorseMvc();
            mvc.Init(services => { });

            mvc.Use(app =>
            {
                IServiceProvider provider = app.GetProvider();
            });

            server.UseMvc(mvc);
            server.Run(5000);
        }
    }
}