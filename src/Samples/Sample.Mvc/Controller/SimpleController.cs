using Horse.Mvc;
using Horse.Mvc.Auth;
using Horse.Mvc.Controllers;
using Horse.Mvc.Filters.Route;

namespace Sample.Mvc.Controller
{
    [Route("api/[controller]")]
    public class SimpleController : HorseController
    {
        [HttpGet("go/{?id}")]
        [Authorize(Roles = "Role1,Role2")]
        public IActionResult Go(int? id)
        {
            return String("Go !");
        }

    }
}
