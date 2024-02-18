using Cashrewards3API.Common.Dto;
using Cashrewards3API.Features.Member.Dto;
using Cashrewards3API.Features.Member.Model;
using Cashrewards3API.Features.Member.Request.SignInMember;
using Cashrewards3API.Features.Member.Request.UpdateCognitoMember;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cashrewards3API.Features.Member.Models;

namespace Cashrewards3API.Features.Member.Interface
{
    public interface IMemberService
    {
        Task<MemberDto> GetMemberById(int memberId);

        Task<MemberModel> GetMemberByCognitoId(int clientId, string memberCognitoId);
        
        Task<SignInMemberInternalResponse> SignInMember(SignInMemberInternalRequest request);

        Task<MemberModel> CreateCognitoMember(MemberModel cognitoMember);

        Task<MemberModel> GetMemberByClientIdAndMemberId(int clientId, int memberId);

        Task<MemberDto> GetMemberByClientIdAndEmail(int clientId, string email);

        Task<CognitoMemberModel> GetCognitoMemberByMemberIdCognitoIdCognitoPoolId(int memberId, Guid cognitoId, string cognitoPoolId);
        Task<CognitoMemberModel> GetCognitoMemberByMemberId(int memberId);

        Task<UpdateFacebookUsernameDto> UpdateFacebookUsername(UpdateFacebookUsernameRequest request);

        Task<string> GetTRAuthToken(MemberContextModel memberContextModel);

        Task<IEnumerable<MemberDto>> GetAssocicatedMembersByMemberIdAsync(int memberId);

        Task<MemberEftposTransformer> GetMemberByIdForEftposTransformer(int memberId);

        Task<Guid?> MapMembersMemberNewIdToMemberNewIdWithClientId(string sourceMemberNewId, int targetClientId);

        Task<EmailMemberDto> GetMemberByEmail(string email);
    }
}