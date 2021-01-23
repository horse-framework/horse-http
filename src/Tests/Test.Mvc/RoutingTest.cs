using FluentAssertions;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Test.Mvc.Arrange;
using Horse.Mvc;
using Horse.Mvc.Controllers;
using Horse.Mvc.Routing;
using Horse.Protocols.Http;
using Xunit;

namespace Test.Mvc
{
    public class RoutingTest
    {
        [Theory]
        [ClassData(typeof(RoutingTestData))]
        public async Task FindRoutes(string method, string path, string aResult)
        {
            HorseMvc mvc = new HorseMvc();
            mvc.Init();
            mvc.CreateRoutes(Assembly.GetExecutingAssembly());
            mvc.Use();

            HttpRequest request = new HttpRequest();
            request.Method = method;
            request.Path = path;

            HttpResponse response = new HttpResponse();

            RouteMatch match = mvc.RouteFinder.Find(mvc.Routes, request);
            Assert.NotNull(match);

            HorseController controller = mvc.ControllerFactory.CreateInstance(mvc, 
                                                                              match.Route.ControllerType, 
                                                                              request, 
                                                                              response, 
                                                                              mvc.ServiceProvider.CreateScope());

            MvcConnectionHandler handler = new MvcConnectionHandler(mvc, null);

            var parameters = (await handler.FillParameters(request, match)).Select(x => x.Value).ToArray();
            Task<IActionResult> task = (Task<IActionResult>) match.Route.ActionType.Invoke(controller, parameters);

            IActionResult result = task.Result;
            string url = Encoding.UTF8.GetString(((MemoryStream) result.Stream).ToArray());
            url.Should().Be(aResult);
        }
    }
}