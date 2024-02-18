using AutoMapper;
using Cashrewards3API.Common.Dto;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Common.Services.Interfaces;
using Cashrewards3API.Common.Utils;
using Cashrewards3API.Exceptions;
using Cashrewards3API.Features.Member.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Net.Http.Headers;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Common
{
    public interface IRequestContext
    {
        string CognitoClientId { get; }

        string CognitoUserId { get; }

        public bool IsMobileDevice { get; }

        int ClientId { get; }

        (int clientId, int? premiumClientId) ClientIds { get; }

        (int clientId, int? premiumClientId) ClientIdsWithoutUserContext { get; }

        bool HasBearerToken { get; }

        MemberContextModel Member { get; }

        string RemoteIpAddress { get; }

        string UserAgent { get; }
        bool IsFromNotifier { get; }

        string GetCognitoUserIdFromAccessToken();

        Task<int> GetClientIdFromDynamoDbAsync();

        Task<int> GetMemberidFromDynamodbasync();
    }

    public class RequestContext : IRequestContext
    {
        private const string TOKEN_CLIENT_ID_CLAIM_NAME = "client_id";
        private const string TOKEN_USERNAME_CLAIM_NAME = "username";
        private const string X_ACCESS_TOKEN_HEADER = "x-access-token";
        private const string EXPIRATIONTOKEN = "exp";
        private const string Notifier = "Notifier";
        private const string UtmSource = "utm_source";
        private readonly IHttpContextAccessor _httpContextAccessor;

        private IClientService _clientService;
        private IMemberService _memberService;
        private readonly IRedisUtil _redisUtil;
        private readonly IMemoryCache _memoryCache;
        private readonly IMapper _mapper;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IPremiumService _premiumService;
        private string _cognitoId;
       

        private string _cognitoClientId;

        private JwtSecurityToken _JWTtoken;

        public bool HasBearerToken => _httpContextAccessor.HttpContext.Request.Headers.ContainsKey(HeaderNames.Authorization);

        public RequestContext(IHttpContextAccessor httpContextAccessor,
                              IClientService clientService,
                              IMemberService memberService,
                              IRedisUtil redisUtil,
                              IMemoryCache memoryCache,
                              IMapper mapper,
                              IPremiumService premiumService,
                              IDateTimeProvider dateTimeProvider
                             )
        {
            _httpContextAccessor = httpContextAccessor;
            _clientService = clientService;
            _memberService = memberService;
            _redisUtil = redisUtil;
            _memoryCache = memoryCache;
            _mapper = mapper;
            _premiumService = premiumService;
            _dateTimeProvider = dateTimeProvider;

        }

        public string CognitoUserId => string.IsNullOrEmpty(_cognitoId) ? _cognitoId = GetValueFromJWTToken(TOKEN_USERNAME_CLAIM_NAME) : _cognitoId;

        public string CognitoClientId => string.IsNullOrEmpty(_cognitoClientId) ? _cognitoClientId = GetValueFromJWTToken(TOKEN_CLIENT_ID_CLAIM_NAME) : _cognitoClientId;

        private int _clientId;

        public int ClientId
        {
            get
            {
                if (_clientId <= 0 && !int.TryParse(_clientService.GetPartner(CognitoClientId).ConfigureAwait(false).GetAwaiter().GetResult(), out _clientId))
                {
                    _clientId = -1;
                }

                return _clientId;
            }
        }

        private (int clientId, int? premiumClientId) _clientIds;

        public (int clientId, int? premiumClientId) ClientIds
        {
            get
            {
                if (_clientIds.clientId == 0)
                {
                    int clientId = HasBearerToken
                        ? GetClientIdFromDynamoDbAsync().ConfigureAwait(false).GetAwaiter().GetResult()
                        : Constants.Clients.CashRewards;
                    var premiumMembership = _premiumService.GetPremiumMembership(clientId, CognitoUserId).ConfigureAwait(false).GetAwaiter().GetResult();
                    var premiumClientId = premiumMembership?.IsCurrentlyActive ?? false ? premiumMembership?.PremiumClientId : null;
                    _clientIds = (clientId, premiumClientId);
                }

                return _clientIds;
            }
        }

        private (int clientId, int? premiumClientId) _clientIdsWithoutUserContext;

        public (int clientId, int? premiumClientId) ClientIdsWithoutUserContext
        {
            get
            {
                if (_clientIdsWithoutUserContext.clientId == 0)
                {
                    int clientId = HasBearerToken
                        ? GetClientIdFromDynamoDbAsync().ConfigureAwait(false).GetAwaiter().GetResult()
                        : Constants.Clients.CashRewards;
                    var premiumClientId = _premiumService.GetPremiumClientId(clientId);
                    _clientIdsWithoutUserContext = (clientId, premiumClientId);
                }

                return _clientIdsWithoutUserContext;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the request comes from a mobile device.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is mobile device; otherwise, <c>false</c>.
        /// </value>
        public bool IsMobileDevice
        {
            get
            {
                bool queryIsMobile = false;
                string device = _httpContextAccessor.HttpContext.Request.Headers["device"];
                bool.TryParse(_httpContextAccessor.HttpContext.Request.Query["IsMobile"], out queryIsMobile);
                string userAgent = _httpContextAccessor.HttpContext.Request.Headers[HeaderNames.UserAgent];

                return (device?.Contains("mobile") ?? false) || queryIsMobile;
            }
        }

        public string GetCognitoUserIdFromAccessToken()
        {
            if (_httpContextAccessor.HttpContext.Request.Headers.ContainsKey(X_ACCESS_TOKEN_HEADER))
            {
                var token = new JwtSecurityToken(_httpContextAccessor.HttpContext.Request.Headers[X_ACCESS_TOKEN_HEADER].ToString());

                return token.Claims.FirstOrDefault(c => c.Type == TOKEN_USERNAME_CLAIM_NAME)?.Value;
            }
            return string.Empty;
        }

        public async Task<int> GetClientIdFromDynamoDbAsync()
        {
            string partnerClientId = await _redisUtil.GetKeyValueAsync($"CongitoClientId:{CognitoClientId}");
            if (partnerClientId == null)
            {
                partnerClientId = await _clientService.GetPartner(CognitoClientId);
                await _redisUtil.SetKeyValueAsync($"CongitoClientId:{CognitoClientId}", partnerClientId, 3600);
            }

            _clientId = -1;
            if (partnerClientId != null)
            {
                _clientId = Convert.ToInt32(partnerClientId);
            }

            return _clientId;
        }

        public async Task<int> GetMemberidFromDynamodbasync()
        {
            int clientId = await GetClientIdFromDynamoDbAsync();
            int memberId = 0;
            if (!string.IsNullOrWhiteSpace(CognitoUserId))
            {
                var memberModel = await _memberService.GetMemberByCognitoId(clientId, CognitoUserId);
                Int32.TryParse(memberModel.MemberId.ToString(), out memberId);
            }
            return memberId;
        }

        private MemberContextModel _member;

        public MemberContextModel Member
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(CognitoUserId) && _member == null)
                {
                    var memberModel = _memberService.GetMemberByCognitoId(ClientId, CognitoUserId).ConfigureAwait(false).GetAwaiter().GetResult();
                    _member = _mapper.Map<MemberContextModel>(memberModel);
                }

                return _member;
            }
        }

        public string RemoteIpAddress
        {
            get
            {
                return _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            }
        }

        public string UserAgent
        {
            get
            {
                return string.Join(';', _httpContextAccessor.HttpContext.Request.Headers["User-Agent"].ToArray());
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is from notifier comming from either query parameter or user agent.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is from notifier; otherwise, <c>false</c>.
        /// </value>
        public bool IsFromNotifier
        {
            get
            {
                return string.Equals(_httpContextAccessor.HttpContext.Request.Query[UtmSource].ToString(), Notifier, StringComparison.OrdinalIgnoreCase)
                    || _httpContextAccessor.HttpContext.Request.Headers[HeaderNames.UserAgent].Any(header=> header.Contains(Notifier,StringComparison.OrdinalIgnoreCase));
                    
            }
        }

        private string GetValueFromJWTToken(string key)
        {
            string result = string.Empty;
            if (_httpContextAccessor.HttpContext.Request.Headers.ContainsKey(HeaderNames.Authorization))
            {
                if (_JWTtoken == null)
                {
                    var accessToken = _httpContextAccessor.HttpContext.Request.Headers[HeaderNames.Authorization];
                    var jwt = accessToken.ToString().Replace("Bearer", string.Empty).Trim();
                    _JWTtoken = new JwtSecurityToken(jwt);
                    IsNotExpiredToken(_JWTtoken);                  
                }
                result = _JWTtoken.Claims.FirstOrDefault(c => c.Type == key)?.Value;
            }
            return result;
        }

        /// <summary>
        /// Determines whether [is not expired token] [the specified token].
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>
        ///   <c>true</c> if [is not expired token] [the specified token]; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="Cashrewards3API.Exceptions.NotAuthorizedException">Expired Token</exception>
        private bool IsNotExpiredToken(JwtSecurityToken token)
        {
            if ((_dateTimeProvider.UtcNow < token.ValidFrom) || (_dateTimeProvider.UtcNow > token.ValidTo))
                throw new NotAuthorizedException("Expired Token");
            return true;
        }
    }
}