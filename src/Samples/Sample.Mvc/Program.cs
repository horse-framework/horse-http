using Sample.Mvc.Models;
using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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
            Console.WriteLine("TMid Middleware");
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
            TwinoMvc mvc = new TwinoMvc();

            mvc.IsDevelopment = false;
            mvc.Init(services =>
            {
                services.AddScoped<IScopedService, ScopedService>();
                services.AddTransient<IFirstService, FirstService>();
                services.AddTransient<ISecondService, SecondService>();

                services.AddJwt(mvc, options =>
                {
                    options.Key = "Very_very_secret_key";
                    options.Issuer = "localhost";
                    options.Audience = "localhost";
                    options.Lifetime = TimeSpan.FromHours(1);
                    options.ValidateAudience = false;
                    options.ValidateIssuer = false;
                    options.ValidateLifetime = true;
                });

                mvc.Policies.Add(Policy.RequireRole("Admin", "Admin"));
                mvc.Policies.Add(Policy.RequireClaims("IT", "Database", "Cloud", "Source"));
                mvc.Policies.Add(Policy.Custom("Custom", (d, c) => true));

                mvc.StatusCodeResults.Add(HttpStatusCode.Unauthorized, new JsonResult(new {Message = "Access denied"}));

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
            server.Start(4410);
            server.BlockWhileRunning();
        }
    }
}