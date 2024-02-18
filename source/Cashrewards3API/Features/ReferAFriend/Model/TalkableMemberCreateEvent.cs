using Newtonsoft.Json;

namespace Cashrewards3API.Features.ReferAFriend.Model
{
    public class TalkableMemberCreateEvent
    {
        public string SiteSlug { get; set; }

        public string Type { get; set; }

        public TalkableMemberCreateEventData Data { get; set; }
    }

    public class TalkableMemberCreateEventData
    {
        public string Email { get; set; }

        public string EventNumber { get; set; }

        public string EventCategory { get; set; }

        public string Uuid { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string CustomerId { get; set; }
    }
}
