using System;
using System.Linq;
using System.Threading.Tasks;
using Horse.Mvc;
using Horse.Mvc.Controllers;
using Horse.Mvc.Filters.Route;

namespace Sample.Mvc.Controller
{
    [Route("a")]
    public class HomeController : HorseController
    {
        [HttpGet("b")]
        public Task<IActionResult> Get()
        {
            throw new NotImplementedException();
        }

        [HttpGet("c")]
        public async Task<IActionResult> GetC()
        {
            return await StringAsync("Welcome!");
        }

        [HttpPost("")]
        public async Task<IActionResult> Post()
        {
            string data = "";
            foreach (var kv in Request.Form)
            {
                data += kv.Key + ": " + kv.Value + "\r\n";
            }

            data += Request.Files.Count() + " Files";
            return await StringAsync(data);
        }
    }
}