﻿using Sample.Mvc.Models;
using System;
using Horse.Mvc;
using Horse.Mvc.Auth;
using Horse.Mvc.Controllers;
using Horse.Mvc.Controllers.Parameters;
using Horse.Mvc.Filters.Route;

namespace Sample.Mvc.Controller
{
    [TestControllerFilter]
    [Route("[controller]")]
    public class OtherController : HorseController
    {
        [HttpGet("hello")]
        public IActionResult Hello([FromQuery] string name)
        {
            return String($"Hello {name}!");
        }

        [HttpGet("go")]
        public IActionResult Go([FromHeader("User-Agent")] string agent)
        {
            return String($"Browser: {agent}!");
        }

        [HttpGet("test")]
        [TestActionFilter]
        public IActionResult Test()
        {
            Console.WriteLine("Test...");
            return String("Hello world!");
        }

        [HttpGet("[action]")]
        public IActionResult act()
        {
            return String("act");
        }

        [HttpGet("complex/[action]/path/{id}/{?opt}")]
        public IActionResult Act(int id, int? opt)
        {
            return String("Complex Act: " + id + ", " + (opt.HasValue ? opt.Value : -1).ToString());
        }

        [Authorize]
        [HttpGet("[action]")]
        public IActionResult Auth()
        {
            return String("auth ok!");
        }

    }
}
