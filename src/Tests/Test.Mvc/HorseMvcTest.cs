using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Test.Mvc.Controllers;
using Horse.Mvc;
using Horse.Protocols.Http;
using Horse.Server;
using Xunit;

namespace Test.Mvc
{
    public class HorseMvcTest
    {
        [Fact]
        public async Task Run()
        {
            HorseMvc mvc = new HorseMvc();

            HomeController cont = new HomeController();
            Assert.NotNull(cont);

            mvc.Init();
            Assembly asm = Assembly.GetExecutingAssembly();
            mvc.CreateRoutes(asm);
            mvc.Use();

            HorseServer server = new HorseServer(ServerOptions.CreateDefault());
            server.UseMvc(mvc, HttpOptions.CreateDefault());
            server.Start(47442);
            System.Threading.Thread.Sleep(1000);

            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync("http://127.0.0.1:47442/home/get");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}