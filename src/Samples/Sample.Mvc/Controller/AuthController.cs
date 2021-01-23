using Sample.Mvc.Models;
using System.Threading.Tasks;
using Horse.Mvc;
using Horse.Mvc.Auth;
using Horse.Mvc.Auth.Jwt;
using Horse.Mvc.Controllers;
using Horse.Mvc.Controllers.Parameters;
using Horse.Mvc.Filters.Route;

namespace Sample.Mvc.Controller
{
    public class AuthController : HorseController
    {
        private IJwtProvider _jwtProvider;

        public AuthController(IJwtProvider jwtProvider)
        {
            _jwtProvider = jwtProvider;
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            JwtToken token = _jwtProvider.Create("1", "mehmet@example.com", null);
            return Json(token);
        }

        [HttpGet("custom")]
        [Authorize("Custom")]
        public IActionResult Custom()
        {
            return String("custom");
        }

        [HttpGet("it")]
        [Authorize("IT")]
        public IActionResult IT()
        {
            return String("IT");
        }

        [HttpPost("post")]
        public Task<IActionResult> Post([FromBody] LoginModel model)
        {
            return JsonAsync(new
                             {
                                 Ok = true,
                                 Code = 200,
                                 Message = "Success: " + model.Username
                             });
        }
    }
}