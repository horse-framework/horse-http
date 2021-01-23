using System.Threading.Tasks;
using Horse.Mvc;
using Horse.Mvc.Controllers;
using Horse.Mvc.Filters.Route;

namespace Sample.Mvc.Controller
{
    [Route("service")]
    public class ServiceController : HorseController
    {
        [HttpGet("get")]
        public async Task<IActionResult> Get()
        {
            return await StringAsync("Service Get");
        }
    }
}