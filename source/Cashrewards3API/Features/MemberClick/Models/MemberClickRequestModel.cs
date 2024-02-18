namespace Cashrewards3API.Features.MemberClick
{
    public class MemberClickRequestModel
    {
        public int? CampaignId { get; set; } 
        
        public string Hyphenated { get; set; }

        public bool IsMobile { get; set; }

        public bool IncludeTiers { get; set; }
    }
}
