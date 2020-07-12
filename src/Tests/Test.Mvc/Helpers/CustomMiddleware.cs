using System.Net;
using System.Threading.Tasks;
using Twino.Mvc;
using Twino.Mvc.Middlewares;
using Twino.Mvc.Results;
using Twino.Protocols.Http;

namespace Test.Mvc.Helpers
{
    public class CustomMiddleware : IMiddleware
    {
        public async Task Invoke(HttpRequest request, HttpResponse response, MiddlewareResultHandler setResult)
        {
            if (request.QueryString.ContainsKey("middleware"))
            {
                string value = request.QueryString["middleware"];
                if (value == "custom")
                {
                    IActionResult result = new StatusCodeResult(HttpStatusCode.Forbidden);
                    setResult(result);
                    return;
                }
            }

            await Task.CompletedTask;
        }
    }
}