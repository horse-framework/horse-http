using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Horse.Mvc.Auth.Jwt
{
    /// <summary>
    /// JWT Horse MVC Configuration class for extension methods
    /// </summary>
    public static class Configuration
    {
        /// <summary>
        /// Adds JWT Implementation to Horse MVC
        /// </summary>
        public static HorseMvc AddJwt(this IServiceCollection services, HorseMvc horse, Action<JwtOptions> options)
        {
            JwtOptions jwtOptions = new JwtOptions();
            options(jwtOptions);

            jwtOptions.SigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));
            horse.ClaimsPrincipalValidator = new JwtClaimsPrincipalValidator(jwtOptions);

            services.AddSingleton(jwtOptions);
            services.AddScoped<IJwtProvider, JwtProvider>();

            return horse;
        }
    }
}