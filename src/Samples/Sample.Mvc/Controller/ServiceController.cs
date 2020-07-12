using System.Threading.Tasks;
using Twino.Mvc;
using Twino.Mvc.Controllers;
using Twino.Mvc.Filters.Route;

namespace Sample.Mvc.Controller
{
    [Route("service")]
    public class ServiceController : TwinoController
    {
        [HttpGet("get")]
        public async Task<IActionResult> Get()
        {
            return await StringAsync("Service Get");
        }
    }
}