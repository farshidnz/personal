using AutoMapper;
using Cashrewards3API.Common.Services.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace Cashrewards3API.Features
{
    [ApiController]
    [Route("api/v1")]
    public class BaseController : ControllerBase
    {
        private const string TOKEN_CLIENT_ID_CLAIM_NAME = "client_id";
        private const string TOKEN_USERNAME_CLAIM_NAME = "username";
        private const string X_ACCESS_TOKEN_HEADER = "x-access-token";

        private UserToken _userToken { get; set; }

        private string _token;

        private string Token
        {
            get
            {
                if (string.IsNullOrEmpty(_token))
                    _token = HttpContextAccessor.HttpContext.Request.Headers[X_ACCESS_TOKEN_HEADER];

                return _token;
            }
        }

        internal UserToken UserToken
        {
            get
            {
                if (_userToken == null)
                    _userToken = new UserToken();
                if (!string.IsNullOrEmpty(Token))
                {
                    var jwt = Token.ToString().Replace("Bearer", string.Empty).Trim();

                    var token = new JwtSecurityToken(jwt);

                    _userToken.ClientId = token.Claims.FirstOrDefault(c => c.Type == TOKEN_CLIENT_ID_CLAIM_NAME)?.Value;
                    _userToken.CognitoId = token.Claims.FirstOrDefault(c => c.Type == TOKEN_USERNAME_CLAIM_NAME)?.Value;
                    _userToken.AccessToken = jwt.ToString();
                }
                return _userToken;
            }
        }

        private IMapper _mapper;

        private IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Gets the mediator.
        /// </summary>
        /// <value>
        /// The mediator.
        /// </value>
        protected IMapper Mapper => _mapper ??= HttpContext.RequestServices.GetService<IMapper>();

        protected IHttpContextAccessor HttpContextAccessor => _httpContextAccessor ??= HttpContext.RequestServices.GetService<IHttpContextAccessor>();
    }
}