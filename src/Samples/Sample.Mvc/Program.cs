﻿using Sample.Mvc.Models;
using System;
using System.Net;
using System.Threading.Tasks;
using Twino.Mvc;
using Twino.Mvc.Auth;
using Twino.Mvc.Auth.Jwt;
using Twino.Mvc.Errors;
using Twino.Mvc.Middlewares;
using Twino.Mvc.Results;
using Twino.Protocols.Http;
using Twino.Server;

namespace Sample.Mvc
{
    public class TMid : IMiddleware
    {
        private IFirstService _firstService;

        public TMid(IFirstService firstService)
        {
            _firstService = firstService;
        }

        public async Task Invoke(HttpRequest request, HttpResponse response, MiddlewareResultHandler setResult)
        {
            Console.WriteLine("tmid");
            await Task.CompletedTask;
        }
    }

    public class MvcErrorHandler : IErrorHandler
    {
        public Task<IActionResult> Error(HttpRequest request, Exception ex)
        {
            IActionResult result = new JsonResult(new {error = true}, HttpStatusCode.InternalServerError);
            return Task.FromResult(result);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            using TwinoMvc mvc = new TwinoMvc();

            mvc.IsDevelopment = false;
            mvc.Init(twino =>
            {
                twino.Services.AddScoped<IScopedService, ScopedService>();
                twino.Services.AddTransient<IFirstService, FirstService>();
                twino.Services.AddTransient<ISecondService, SecondService>();

                twino.AddJwt(options =>
                {
                    options.Key = "Very_very_secret_key";
                    options.Issuer = "localhost";
                    options.Audience = "localhost";
                    options.Lifetime = TimeSpan.FromHours(1);
                    options.ValidateAudience = false;
                    options.ValidateIssuer = false;
                    options.ValidateLifetime = true;
                });

                twino.Policies.Add(Policy.RequireRole("Admin", "Admin"));
                twino.Policies.Add(Policy.RequireClaims("IT", "Database", "Cloud", "Source"));
                twino.Policies.Add(Policy.Custom("Custom", (d, c) => true));

                twino.StatusCodeResults.Add(HttpStatusCode.Unauthorized, new JsonResult(new {Message = "Access denied"}));

                mvc.ErrorHandler = new MvcErrorHandler();
            });

            CorsMiddleware cors = new CorsMiddleware();
            cors.AllowAll();

            mvc.Use(app =>
            {
                app.UseMiddleware(cors);
                app.UseMiddleware<TMid>();
                app.UseFiles("/download", "/home/mehmet/files");
            });

            TwinoServer server = new TwinoServer();
            var opt = HttpOptions.CreateDefault();
            opt.HttpConnectionTimeMax = 0;
            server.UseMvc(mvc, opt);
            server.Start(441);
            server.BlockWhileRunning();
        }
    }
}