

### Basic Example

Twino.Mvc similar to ASP.NET Core. Here is a basic example:

    class Program
    {
        static void Main(string[] args)
        {
            TwinoMvc mvc = new TwinoMvc();
            mvc.Init();
            mvc.Use(app =>
            {
                app.UseMiddleware<CorsMiddleware>();
            });

            TwinoServer server = new TwinoServer();
            server.UseMvc(mvc, HttpOptions.CreateDefault());
            server.Start();
            
            //optional
            server.BlockWhileRunning();
        }
    }

    [Route("[controller]")]
    public class DemoController : TwinoController
    {
        [HttpGet("get/{?id}")]
        public async Task<IActionResult> Get([FromRoute] int? id)
        {
            return await StringAsync("Hello world: " + id);
        }

        [HttpPost("get2")]
        public async Task<IActionResult> Get2([FromBody] CustomModel model)
        {
            return await JsonAsync(new {Message = "Hello World Json"});
        }
    }
