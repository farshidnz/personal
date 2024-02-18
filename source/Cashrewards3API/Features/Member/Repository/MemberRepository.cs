using Cashrewards3API.Common.Services;
using Cashrewards3API.Features.Member.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cashrewards3API.Features.Member.Models;
using Cashrewards3API.Features.Member.Request.UpdateCognitoMember;

namespace Cashrewards3API.Features.Member.Repository
{
    public interface IMemberRepository
    {
        Task<MemberModel> GetMemberModelByEmailAndClientId(string email, int clientId);
        Task<MemberInternalModel> GetMemberInternalModelByEmailAndClientId(string email, int clientId);
        Task<MemberModel> GetMemberModelByFacebookUsernameAndClientId(string facebookUsername, int clientId);
        Task UpdateFacebookUsernameIntoDb(int memberId, string facebookUsername);
        Task<IEnumerable<MemberModel>> GetAssocicatedMembersByMemberId(IEnumerable<int> memberIds);
        Task<IEnumerable<int>> GetAssociatedMemberIds(int memberId);
        Task<IEnumerable<MemberEftposTransformer>> GetMemberByIdForEftposTransformer(int memberId);
    }

    public class MemberRepository : IMemberRepository
    {
        private readonly IRepository _repository;
        private readonly IReadOnlyRepository _readOnlyRepository;

        public MemberRepository(IRepository repository, IReadOnlyRepository readOnlyRepository)
        {
            _repository = repository;
            _readOnlyRepository = readOnlyRepository;
        }

        public async Task<MemberModel> GetMemberModelByEmailAndClientId(string email, int clientId)
        {
            var queryString = @"SELECT *
                                FROM dbo.Member
                                WHERE email = @email and clientid = @clientId";

            return await _readOnlyRepository.QueryFirstOrDefault<MemberModel>(queryString, new { email, clientId });
        }

        public async Task<MemberInternalModel> GetMemberInternalModelByEmailAndClientId(string email, int clientId)
        {
            var queryString = @"SELECT MemberNewId,MemberId,FirstName,LastName,PostCode,AccessCode,Mobile,Status,UserPassword,SaltKey
                                FROM dbo.Member
                                WHERE email = @email and clientid = @clientId";

            return await _readOnlyRepository.QueryFirstOrDefault<MemberInternalModel>(queryString, new { email, clientId });
        }

        public async Task<MemberModel> GetMemberModelByFacebookUsernameAndClientId(string facebookUsername, int clientId)
        {
            var queryString = @"SELECT *
                                FROM dbo.Member
                                WHERE facebookUsername = @facebookUsername and clientid = @clientId";

            return await _readOnlyRepository.QueryFirstOrDefault<MemberModel>(queryString, new { facebookUsername, clientId });
        }


        public async Task UpdateFacebookUsernameIntoDb(int memberId, string facebookUsername)
        {
            var queryString = @"Update Member SET FacebookUsername = @facebookUsername
                                WHERE MemberId =@memberId";

            await _repository.Execute(queryString, new { memberId, facebookUsername });
        }

        public async Task<IEnumerable<MemberModel>> GetAssocicatedMembersByMemberId(IEnumerable<int> memberIds)
        {
            var queryString = @"SELECT ClientId, M.MemberId, FirstName, LastName, M.MemberNewId, AccessCode, Email, 
                                       ClientId, DateJoined, PostCode, Mobile, CampaignId, 
                                       Source, IsValidated, ReceiveNewsLetter,
                                       C.CognitoPoolId, C.CognitoId CognitoIdString, P.PremiumStatus, C.PersonId, p.OriginationSource
                                FROM dbo.Member M WITH (NOLOCK)
                                LEFT JOIN dbo.CognitoMember C on M.MemberId = C.MemberId
                                LEFT OUTER JOIN dbo.Person P on C.PersonId = P.PersonId
                                WHERE M.MemberId in @memberIds
                                ";

            return await _readOnlyRepository.QueryAsync<MemberModel>(queryString, new { memberIds });
        }

        public async Task<IEnumerable<int>> GetAssociatedMemberIds(int memberId)
        {
            var query = @"SELECT memberId from CognitoMember 
                        WHERE CognitoId = 
                        (SELECT CognitoId FROM CognitoMember Where MemberId = @memberId) ";
            return await _readOnlyRepository.QueryAsync<int>(query, new { memberId });
        }

        public async Task<IEnumerable<MemberEftposTransformer>> GetMemberByIdForEftposTransformer(int memberId)
        {
            string getMemberIdFromPersonIdAndClientIdQuery = @"
                SELECT TOP 1 
                    Member.MemberId AS MemberId,
                    Member.ClientId AS ClientId,
                    Member.Status AS Status,
                    Person.PersonId AS PersonId,
                    Person.CognitoId AS CognitoId,
                    Person.PremiumStatus AS PremiumStatus
                    FROM Member LEFT JOIN Person on Member.PersonId = Person.PersonId
                    WHERE MemberId = @MemberId;
            ";

            return await _readOnlyRepository.QueryAsync<MemberEftposTransformer>(getMemberIdFromPersonIdAndClientIdQuery, new
            {
                MemberId = memberId,
            });
        }
    }
}
