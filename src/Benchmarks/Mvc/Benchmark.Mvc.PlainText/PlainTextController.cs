using System.Threading.Tasks;
using Horse.Mvc;
using Horse.Mvc.Controllers;
using Horse.Mvc.Filters.Route;

namespace Benchmark.Mvc.PlainText
{
    [Route("plaintext")]
    public class PlainTextController : HorseController
    {
        [HttpGet]
        public Task<IActionResult> Get()
        {
            return StringAsync("Hello, World!");
        }
    }
}