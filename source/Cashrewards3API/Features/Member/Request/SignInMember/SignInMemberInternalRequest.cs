namespace Cashrewards3API.Features.Member.Request.SignInMember
{
    public class SignInMemberInternalRequest
    {
        public SignInMemberInternalRequest(string email, string password)
        {
            Email = email;
            Password = password;
        }

        public string Email { get; set; }

        public string Password { get; set; }
    }
}
