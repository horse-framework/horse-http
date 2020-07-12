﻿using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Twino.Protocols.Http;

namespace Twino.Mvc.Auth.Jwt
{
    /// <summary>
    /// After HTTP Request is read, before to process the request.
    /// If a claim principal validator is registered
    /// Get method is called to read authorization and reads Bearer Token for creation JWT ClaimsPrincipal.
    /// </summary>
    public class JwtClaimsPrincipalValidator : IClaimsPrincipalValidator
    {
        /// <summary>
        /// JSON Web Token Options from ServiceContainer
        /// </summary>
        public JwtOptions Options { get; }

        /// <summary>
        /// Creates new JWT token validator with specified options
        /// </summary>
        public JwtClaimsPrincipalValidator(JwtOptions options)
        {
            Options = options;
        }

        /// <summary>
        /// Reads the Request and creates ClaimsPrincipal data if user has valid JWT token
        /// </summary>
        public ClaimsPrincipal Get(HttpRequest request)
        {
            if (!request.Headers.ContainsKey(HttpHeaders.AUTHORIZATION))
                return null;

            string schemeAndToken = request.Headers[HttpHeaders.AUTHORIZATION];
            return Get(schemeAndToken);
        }

        /// <summary>
        /// Reads the token string and creates ClaimsPrincipal data if user has valid token or another authentication info.
        /// Token string must be in "Bearer xxx.." form
        /// </summary>
        public ClaimsPrincipal Get(string token)
        {
            ClaimsPrincipal principal = GetToken(token, out _);
            return principal;
        }

        /// <summary>
        /// Creates ClaimsPrincipal from token
        /// </summary>
        public ClaimsPrincipal GetToken(string token, out SecurityToken validatedToken)
        {
            string[] schemeAndToken = token.Split(' ');
            string scheme;
            string value;

            if (schemeAndToken.Length == 1)
            {
                value = schemeAndToken[0];
            }
            else
            {
                scheme = schemeAndToken[0];
                value = schemeAndToken[1];

                if (!string.Equals(scheme, HttpHeaders.BEARER, StringComparison.InvariantCultureIgnoreCase))
                {
                    validatedToken = null;
                    return null;
                }
            }

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            tokenHandler.InboundClaimTypeMap.Clear();
            TokenValidationParameters validationParameters = new TokenValidationParameters
                                                             {
                                                                 ValidateLifetime = Options.ValidateLifetime,
                                                                 ValidateAudience = Options.ValidateAudience,
                                                                 ValidateIssuer = Options.ValidateIssuer,
                                                                 ValidIssuer = Options.Issuer,
                                                                 ValidAudience = Options.Audience,
                                                                 IssuerSigningKey = Options.SigningKey
                                                             };

            ClaimsPrincipal principal = tokenHandler.ValidateToken(value, validationParameters, out validatedToken);
            return principal;
        }
    }
}