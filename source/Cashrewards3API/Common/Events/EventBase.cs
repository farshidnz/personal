using System;
using System.Collections.Generic;

namespace Cashrewards3API.Common.Events
{
    public class EventBase
    {
        public EventBase()
        {
            this.Event = new Event();
        }

        public int MemberId { get; set; }

        public string ExternalMemberId { get; set; }

        public Event Event { get; set; }
    }

    public class Event
    {
        public virtual string Name { get; set; }

        public long Time => ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();

        public Dictionary<string, string> Params { get; set; } = new Dictionary<string, string>();
    }
}