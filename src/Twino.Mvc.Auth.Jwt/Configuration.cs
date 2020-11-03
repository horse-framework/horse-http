using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Twino.Mvc.Auth.Jwt
{
    /// <summary>
    /// JWT Twino MVC Configuration class for extension methods
    /// </summary>
    public static class Configuration
    {
        /// <summary>
        /// Adds JWT Implementation to Twino MVC
        /// </summary>
        public static TwinoMvc AddJwt(this IServiceCollection services, TwinoMvc twino, Action<JwtOptions> options)
        {
            JwtOptions jwtOptions = new JwtOptions();
            options(jwtOptions);

            jwtOptions.SigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));
            twino.ClaimsPrincipalValidator = new JwtClaimsPrincipalValidator(jwtOptions);

            services.AddSingleton(jwtOptions);
            services.AddScoped<IJwtProvider, JwtProvider>();

            return twino;
        }
    }
}