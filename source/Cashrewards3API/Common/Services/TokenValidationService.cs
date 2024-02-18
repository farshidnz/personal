using Cashrewards3API.Common.Configuration;
using Cashrewards3API.Common.Services.Interfaces;
using Cashrewards3API.Exceptions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

namespace Cashrewards3API.Common.Services
{
    public class TokenValidationService : ITokenValidation
    {
        private readonly AWSInfrastructureSettings _configuration;

        private readonly IMemoryCache _memoryCache;

        public TokenValidationService(AWSInfrastructureSettings configuration,
        IMemoryCache memoryCache)
        {
            _configuration = configuration;
            _memoryCache = memoryCache;
        }

        /// <summary>
        /// Validates the token .
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <returns></returns>
        /// <exception cref="Cashrewards3API.Exceptions.NotAuthorizedException"></exception>
        public JwtSecurityToken ValidateToken(string accessToken)
        {
            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = null;
            try
            {
                handler.ValidateToken(accessToken, GetTokenValidationParameters(), out SecurityToken validatedToken);
                jwtToken = (JwtSecurityToken)validatedToken;

                if (jwtToken == null)
                    throw new NotAuthorizedException();
            }
            catch (Exception ex)
            {
                throw new NotAuthorizedException(ex.Message);
            }
            return jwtToken;
        }

        private TokenValidationParameters GetTokenValidationParameters()
        {
            var region = _configuration.Region;
            var poolId = _configuration.UserPoolId;

            return new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKeyResolver = (s, securityToken, identifier, parameters) =>
                {
                    string issuerToken = (string)_memoryCache.Get(parameters.ValidIssuer + "/.well-known/jwks.json");
                    if (string.IsNullOrEmpty(issuerToken))
                        if (!_memoryCache.TryGetValue<string>(parameters.ValidIssuer + "/.well-known/jwks.json", out issuerToken))
                        {
                            issuerToken = new WebClient().DownloadString(parameters.ValidIssuer + "/.well-known/jwks.json");

                            var cacheEntryOptions = new MemoryCacheEntryOptions()
                                .SetSlidingExpiration(TimeSpan.FromSeconds(6000));

                            _memoryCache.Set(parameters.ValidIssuer + "/.well-known/jwks.json", issuerToken, cacheEntryOptions);
                        }
                    return JsonConvert.DeserializeObject<JsonWebKeySet>(issuerToken).Keys;
                },
                ValidateIssuer = true,
                ValidIssuer = $"https://cognito-idp.{region}.amazonaws.com/{poolId}",
                ValidateAudience = false,
                ValidateLifetime = true
            };
        }
    }
}