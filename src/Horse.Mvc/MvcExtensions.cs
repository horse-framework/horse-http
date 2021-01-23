using Horse.Core;
using Horse.Protocols.Http;

namespace Horse.Mvc
{
    /// <summary>
    /// Extension methods for Horse MVC
    /// </summary>
    public static class MvcExtensions
    {
        /// <summary>
        /// Uses HTTP Protocol and accepts HTTP connections with Horse MVC Architecture
        /// </summary>
        public static IHorseServer UseMvc(this IHorseServer server, HorseMvc mvc, string optionsFilename)
        {
            return UseMvc(server, mvc, HttpOptions.Load(optionsFilename));
        }

        /// <summary>
        /// Uses HTTP Protocol and accepts HTTP connections with Horse MVC Architecture
        /// </summary>
        public static IHorseServer UseMvc(this IHorseServer server, HorseMvc mvc)
        {
            return UseMvc(server, mvc, HttpOptions.CreateDefault());
        }

        /// <summary>
        /// Uses HTTP Protocol and accepts HTTP connections with Horse MVC Architecture
        /// </summary>
        public static IHorseServer UseMvc(this IHorseServer server, HorseMvc mvc, HttpOptions options)
        {
            MvcConnectionHandler handler = new MvcConnectionHandler(mvc, mvc.AppBuilder);
            HorseHttpProtocol protocol = new HorseHttpProtocol(server, handler, options);
            server.UseProtocol(protocol);
            return server;
        }
    }
}