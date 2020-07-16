# Twino MVC

Twino MVC is an HTTP Server with MVC Architecture. It's usage is very similar to ASP.NET Core and faster. You can also ise core Protocols.Http library for basic HTTP server without MVC.

## NuGet Packages

**[Twino MVC](https://www.nuget.org/packages/Twino.Mvc)**<br>
**[Twino MVC JWT Auth Library](https://www.nuget.org/packages/Twino.Mvc.Auth.Jwt)**<br>
**[Twino HTTP Server](https://www.nuget.org/packages/Twino.Protocols.Http)**<br>


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
