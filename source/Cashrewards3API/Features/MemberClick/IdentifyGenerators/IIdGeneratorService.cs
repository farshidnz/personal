namespace Cashrewards3API.Features.MemberClick
{
    public interface IIdGeneratorService
    {
        string GetUniqueId(int memberId, int clientId);
    }
}
