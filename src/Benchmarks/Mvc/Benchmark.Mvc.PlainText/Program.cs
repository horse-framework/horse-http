using System;
using Twino.Mvc;
using Twino.Server;

namespace Benchmark.Mvc.PlainText
{
    class Program
    {
        static void Main(string[] args)
        {
            TwinoServer server = new TwinoServer();
            
            TwinoMvc mvc = new TwinoMvc();
            mvc.Init(services =>
            {

            });

            mvc.Use(app =>
            {
                IServiceProvider provider = app.GetProvider();
            });
            
            server.UseMvc(mvc);
            
            server.Start(5000);
            server.BlockWhileRunning();
        }
    }
}