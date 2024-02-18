using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Cashrewards3API.Common.Configuration
{
    public class JwtAuthentication
    {
        public string SecurityKey { get; set; }
        public string ValidIssuer { get; set; }
        public string ValidAudience { get; set; }

        public SymmetricSecurityKey SymmetricSecurityKey => new SymmetricSecurityKey(Convert.FromBase64String(SecurityKey));
        public SigningCredentials SigningCredentials => new SigningCredentials(SymmetricSecurityKey, SecurityAlgorithms.HmacSha256);
    }

    public class ConfigureJwtBearerOptions : IPostConfigureOptions<JwtBearerOptions>
    {
        private readonly IOptions<JwtAuthentication> _jwtAuthentication;
        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _configuration;

        public ConfigureJwtBearerOptions(IOptions<JwtAuthentication> jwtAuthentication, IMemoryCache memoryCache, IConfiguration configuration)
        {
            _jwtAuthentication = jwtAuthentication ?? throw new System.ArgumentNullException(nameof(jwtAuthentication));
            _memoryCache = memoryCache;
            _configuration = configuration;
        }

        public void PostConfigure(string name, JwtBearerOptions options)
        {

            var jwtAuthentication = _jwtAuthentication.Value;
            var region = _configuration["AWS:Region"];
            var poolId = _configuration["AWS:UserPoolId"];

            options.ClaimsIssuer = jwtAuthentication.ValidIssuer;
            options.IncludeErrorDetails = true;
            options.RequireHttpsMetadata = true;
            options.TokenValidationParameters = new TokenValidationParameters
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
                ValidateAudience = false
            };
        }
    }
}
