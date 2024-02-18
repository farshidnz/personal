namespace Cashrewards3API.Features.Member.Model
{
    public class ClientMemberResultModel
    {
        public int MemberId { get; set; }
        public System.Guid MemberNewId { get; set; }
        public string Email { get; set; }
        public int ClientId { get; set; }
    }
}