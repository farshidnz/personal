namespace Cashrewards3API.Common.Events
{
    public class MemberPremiumUpdateEvent : EventBase
    {
        public MemberPremiumUpdateEvent()
        {
            this.Event.Name = "PremiumMembership";
        }     
    }
}