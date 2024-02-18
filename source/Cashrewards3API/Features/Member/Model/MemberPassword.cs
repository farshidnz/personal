namespace Cashrewards3API.Features.Member.Model
{
    public class MemberPassword
    {
        public int MemberId { get; set; }

        public string PlainPassword { get; set; }

        public string SaltKey { get; set; }

        public string UserPassword { get; set; }

        public string HashedEmail { get; set; }

        public string HashedMobile { get; set; }
    }
}