using System;
using System.Net;
using Test.Mvc.Models;
using Horse.Mvc;
using Horse.Mvc.Controllers;
using Horse.Mvc.Controllers.Parameters;
using Horse.Mvc.Filters.Route;
using Horse.Mvc.Results;

namespace Test.Mvc.Controllers
{
    [Route("[controller]")]
    public class HomeController : HorseController
    {
        [HttpGet("get")]
        public IActionResult Get()
        {
            return Json(new { Ok = true, Message = "Hello world", Code = 200 });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
                return new StatusCodeResult(HttpStatusCode.BadRequest);

            if (model.Username == "root" && model.Password == "password")
                return Json(new { Ok = true, Message = "Welcome", Code = 200 });

            return Json(new { Ok = false, Message = "Unauthorized", Code = 401 });
        }

        [HttpPut("login-form")]
        public IActionResult LoginForm([FromForm] LoginModel model)
        {
            throw new NotImplementedException();
        }

        [HttpDelete("remove/{id}")]
        public IActionResult Remove(int id)
        {
            throw new NotImplementedException();
        }
    }
}