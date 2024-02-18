namespace Cashrewards3API.Common.Services.Model
{
    public class UserToken
    {
        public string CognitoId { get; set; }

        public string AccessToken { get; set; }

        public string ClientId { get; set; }
    }
}