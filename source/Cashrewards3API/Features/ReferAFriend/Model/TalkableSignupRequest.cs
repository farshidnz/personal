namespace Cashrewards3API.Features.ReferAFriend.Model
{
    public class TalkableSignupRequest
    {
        public string Email { get; set; }

        public int MemberId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string TalkableUuid { get; set; }
    }
}
