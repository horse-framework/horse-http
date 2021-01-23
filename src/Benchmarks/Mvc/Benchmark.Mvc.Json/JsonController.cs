using System.Threading.Tasks;
using Horse.Mvc;
using Horse.Mvc.Controllers;
using Horse.Mvc.Filters.Route;

namespace Benchmark.Mvc.Json
{
    [Route("json")]
    public class JsonController : HorseController
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return await JsonAsync(new { message = "Hello, World!" });
        }
    }
}