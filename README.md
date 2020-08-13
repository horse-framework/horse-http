# Twino MVC

[![NuGet](https://img.shields.io/nuget/v/Twino.Mvc?label=mvc%20nuget)](https://www.nuget.org/packages/Twino.Mvc)
[![NuGet](https://img.shields.io/nuget/v/Twino.Mvc.Auth.Jwt?label=jwt%20extension%20nuget)](https://www.nuget.org/packages/Twino.Mvc.Auth.Jwt)
[![NuGet](https://img.shields.io/nuget/v/Twino.Protocols.Http?label=http%20server%20nuget)](https://www.nuget.org/packages/Twino.Protocols.Http)

Twino MVC is an HTTP Server with MVC Architecture. It's usage is very similar to ASP.NET Core and faster. You can also ise core Protocols.Http library for basic HTTP server without MVC.


### Basic Example

    class Program
    {
        static void Main(string[] args)
        {
            TwinoMvc mvc = new TwinoMvc();
            mvc.Init(m =>
            {
                //initialization
                m.Services.AddTransient<ISampleService, SampleService>();
            });
            mvc.Use(app =>
            {
                app.UseMiddleware<SampleMiddleware>();
            });

            //create new twino server
            TwinoServer server = new TwinoServer();
            
            //bind MVC to the server
            server.UseMvc(mvc, HttpOptions.CreateDefault());
            
            //start server
            server.Start();
            
            //server runs async, if you want to block the current thread
            server.BlockWhileRunning();
        }
    }

    [Route("[controller]")]
    public class DemoController : TwinoController
    {
        [HttpGet("get/{?id}")]
        public Task<IActionResult> Get([FromRoute] int? id)
        {
            return StringAsync("Hello world: " + id);
        }

        [HttpPost("get2")]
        public async Task<IActionResult> Get2([FromBody] CustomModel model)
        {
            return await JsonAsync(new {Message = "Hello World Json"});
        }
    }


### JWT Example

    class Program
    {
        static void Main(string[] args)
        {
            TwinoMvc mvc = new TwinoMvc();
            mvc.Init(m =>
            {
                //initialization
                mvc.AddJwt(o =>
                {
                    o.Audience = "your_company";
                    o.Issuer = "yoursite.com";
                    o.ValidateAudience = false;
                    o.ValidateIssuer = true;
                    o.ValidateLifetime = true;
                    o.Key = "your_secret";
                    o.Lifetime = TimeSpan.FromHours(1);
                });
            });
            
            mvc.Use(app =>
            {
                app.UseMiddleware(cors);
            });

            //create new twino server
            TwinoServer server = new TwinoServer();
            
            //bind MVC to the server
            server.UseMvc(mvc, HttpOptions.CreateDefault());
            
            //start server
            server.Start();
            
            //server runs async, if you want to block the current thread
            server.BlockWhileRunning();
        }
    }
    
    [Route("[controller]")]
    public class DemoController : TwinoController
    {
        [HttpGet]
        [Authorize]
        public Task<IActionResult> Get()
        {
            //you can use User property of TwinoController
            
            return StringAsync("Hello, World!");
        }
    }


### CORS and Some Useful Options

    class Program
    {
        static void Main(string[] args)
        {
            TwinoMvc mvc = new TwinoMvc();
            
            mvc.Init();
            
            //create cors service and allow all (not related to jwt)
            CorsMiddleware cors = new CorsMiddleware();
            cors.AllowAll();
            
            mvc.Use(app =>
            {
                app.UseMiddleware(cors);
            });

            //use global result for internal errors
            mvc.StatusCodeResults.Add(HttpStatusCode.InternalServerError, new JsonResult(new SampleResult {Ok = false, Code = "Error"}));
            
            //use global result for unauthorized results
            mvc.StatusCodeResults.Add(HttpStatusCode.Unauthorized, new JsonResult(new SampleResult {Ok = false, Code = "Unauthorized"}));

            //use custom handler for errors (CustomErrorHandler implements IErrorHandler)
            mvc.ErrorHandler = new CustomErrorHandler();
            
            //create new twino server
            TwinoServer server = new TwinoServer();
            
            //bind MVC to the server
            server.UseMvc(mvc, HttpOptions.CreateDefault());
            
            //start server
            server.Start();
            
            //server runs async, if you want to block the current thread
            server.BlockWhileRunning();
        }
    }


## Thanks

Thanks to JetBrains for open source license to use on this project.

[![jetbrains](https://user-images.githubusercontent.com/21208762/90192662-10043700-ddcc-11ea-9533-c43b99801d56.png)](https://www.jetbrains.com/?from=twino-framework)
