using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Features.ReferAFriend.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.ReferAFriend
{
    public interface ITalkableService
    {
        Task<TalkableSignupResult> SignUp(TalkableSignupRequest request);
    }

    public class TalkableService : ITalkableService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IMapper _mapper;
        private readonly string _talkableEnvironment;
        private readonly string _talkableApiKey;
        private readonly CommonConfig _config;
        private readonly HttpClient _httpClient;

        public TalkableService(
            IMapper mapper,
             CommonConfig config, IHttpClientFactory httpClientFactory)
        {
            _clientFactory = httpClientFactory;
            _httpClient = _clientFactory.CreateClient("talkable"); ;
            _config = config;
            _mapper = mapper;
            _talkableEnvironment = config.Talkable.Environment;
            _talkableApiKey = config.Talkable.ApiKey;
        }

        public async Task<TalkableSignupResult> SignUp(TalkableSignupRequest request)
        {
            var talkableSignupResult = await CreateSignUp(request);
            if (!talkableSignupResult)
            {
                throw new Exception($"Signup to talkable failed, Email:{request.Email}, MemberId:{request.MemberId}, Uuid:{request.TalkableUuid}");
            }

            return new TalkableSignupResult { Status = talkableSignupResult };
        }

        private async Task<bool> CreateSignUp(TalkableSignupRequest request)
        {
            var signUEvent = CreateSignupEventPayload(request);
            var response = await SendSignUpRequest(signUEvent).ConfigureAwait(false);

            return (bool)JObject.Parse(response)["ok"];
        }

        private TalkableMemberCreateEvent CreateSignupEventPayload(TalkableSignupRequest request) =>
            _mapper.Map<TalkableMemberCreateEvent>(request, opts =>
            {
                opts.Items[Constants.Mapper.SiteSlug] = _talkableEnvironment;
                opts.Items[Constants.Mapper.Type] = "Event";
                opts.Items[Constants.Mapper.EventCategory] = "signup";
            });

        private async Task<string> SendSignUpRequest(TalkableMemberCreateEvent signUpEvent)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _talkableApiKey);

            var json = SerializeToSnakeCase(signUpEvent);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using (var response = await _httpClient.PostAsync("v2/origins", content))
            {
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
        }

        private static string SerializeToSnakeCase(TalkableMemberCreateEvent signUpEvent)
        {
            return JsonConvert.SerializeObject(signUpEvent, new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            });
        }
    }
}