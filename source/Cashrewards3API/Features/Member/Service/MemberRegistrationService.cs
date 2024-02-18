using Amazon.SimpleNotificationService;
using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Common.Dto;
using Cashrewards3API.Common.Services.Interfaces;
using Cashrewards3API.Features.Member.Interface;
using Cashrewards3API.Features.Member.Model;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using NotFoundException = Cashrewards3API.Exceptions.NotFoundException;

namespace Cashrewards3API.Features.Member.Service
{
    public class MemberRegistrationService : IMemberRegistrationService
    {
        private readonly ShopgoDBContext _shopGoDbContext;
        private readonly IMemberService _memberService;
        private readonly ILogger<MemberRegistrationService> _logger;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IDateTimeProvider _dateTimeProvider;

        public MemberRegistrationService(ShopgoDBContext shopGoDBContext,
        IMemberService memberService,
        ILogger<MemberRegistrationService> logger,
        IAmazonSimpleNotificationService snsClient,
        IMapper mapper,
        IConfiguration configuration,
        IDateTimeProvider datetimeProvider)
        {
            _shopGoDbContext = shopGoDBContext;
            _memberService = memberService;
            _logger = logger;
            _mapper = mapper;
            _configuration = configuration;
            _dateTimeProvider = datetimeProvider;
        }

        public async Task<ClientMemberResultModel> CreateBlueMemberFromCashRewardsMember(string cognitoId)
        {
            var member = await _memberService.GetMemberByCognitoId(Constants.Clients.CashRewards, cognitoId);
            if (member == null)
                throw new NotFoundException(
                    $"Member for CognitoId: {cognitoId}  not exist for CashRewards client");

            MemberDto existingBlueMember = await _memberService.GetMemberByClientIdAndEmail(Constants.Clients.Blue, member.Email);
            if (existingBlueMember != null)
                return _mapper.Map<ClientMemberResultModel>(existingBlueMember);

            member.ClientId = Constants.Clients.Blue;
            member.MemberNewId = Guid.NewGuid();
            member.DateJoined = _dateTimeProvider.Now;
            
            member = await CreateMemberFromExistingMember(member, cognitoId);
            return _mapper.Map<ClientMemberResultModel>(member);
        }

        #region db queries

        public async Task<MemberModel> CreateMemberFromExistingMember(MemberModel member, string cognitoId)
        {
            await using var conn = _shopGoDbContext.CreateConnection();
            conn.Open();
            await using var transaction = conn.BeginTransaction();
            MemberModel createBlueMember = await InsertMemberToDbFromExistingMember(member, conn, transaction);
            var cognitoMemberModel = _mapper.Map<CognitoMemberModel>(createBlueMember);
            await InsertCognitoMemberMappingToDb(cognitoMemberModel, conn, transaction);
            await transaction.CommitAsync();
            return createBlueMember;
        }

        public async Task<MemberModel> InsertMemberToDbFromExistingMember(MemberModel member,
            SqlConnection conn,
            SqlTransaction transaction)
        {
            const string insertBlueMemberSqlQuery = @"
                INSERT INTO Member (ClientId, Status,DateJoined,ActivateBy,
                FirstName,LastName,PostCode,Mobile,ReceiveNewsLetter,Email,CookieIpAddress,AccessCode,
                UserPassword,SaltKey,HashedEmail,HashedMobile,MemberNewId,DateOfBirth,CampaignId,
                Source,MobileSHA256, FacebookUsername, Gender, SmsConsent, AppNotificationConsent,CommsPromptShownCount, PopUpActive,
                IsValidated, IsResetPassword, RequiredLogin, IsAvailable, DateDeletedByMember,
                MailChimpListEmailID, DateReceiveNewsLetter, CommunicationsEmail,
                HashedMemberNewId, AutoCreated, PaypalEmail, TwoFactorAuthyId, IsTwoFactorAuthEnabled,
                TwoFactorAuthActivationToken, TwoFactorAuthActivateBy, TwoFactorAuthActivationMobile, TwoFactorAuthActivationCountryCode,
                RiskDescription, IsRisky, Comment, InstallNotifier, KycStatusId, ClickWindowActive, PersonId)
                SELECT
                @ClientId, Status, @DateJoined, ActivateBy,
                FirstName, LastName, PostCode,  Mobile, ReceiveNewsLetter, Email, CookieIpAddress, AccessCode,
                UserPassword, SaltKey, HashedEmail, HashedMobile, @MemberNewId, DateOfBirth, CampaignId,
                Source, MobileSHA256, FacebookUsername, Gender, SmsConsent, AppNotificationConsent,CommsPromptShownCount, PopUpActive,
                IsValidated, IsResetPassword, RequiredLogin, IsAvailable, DateDeletedByMember,
                MailChimpListEmailID, DateReceiveNewsLetter, CommunicationsEmail,
                HashedMemberNewId, AutoCreated, PaypalEmail, TwoFactorAuthyId, IsTwoFactorAuthEnabled,
                TwoFactorAuthActivationToken, TwoFactorAuthActivateBy, TwoFactorAuthActivationMobile, TwoFactorAuthActivationCountryCode,
                RiskDescription, IsRisky, Comment, InstallNotifier, KycStatusId, ClickWindowActive, PersonId
                FROM Member WHERE memberId=@MemberId;
                SELECT CAST(SCOPE_IDENTITY() as int);
            ";

            var insertedResult = await conn.QueryAsync<int>(insertBlueMemberSqlQuery,
                    new
                    {
                        MemberId = member.MemberId,
                        MemberNewId = member.MemberNewId,
                        ClientId = member.ClientId,
                        DateJoined = member.DateJoined
                    }, transaction);
            member.MemberId = insertedResult.Single();
            return member;
        }

        public async Task InsertCognitoMemberMappingToDb(CognitoMemberModel cognitoMemberModel,
            SqlConnection conn,
            SqlTransaction transaction)
        {
            const string insertCognitoMemberSqlQuery = @"
                INSERT into CognitoMember(CognitoId, MemberId, CognitopoolId, MemberNewId, Status, PersonId)
                values (@CognitoId, @MemberId, @CognitopoolId, @MemberNewId, @Status, @PersonId);
            ";
            await conn.QueryAsync<int>(insertCognitoMemberSqlQuery,
                    cognitoMemberModel, transaction);
        }
    }

    #endregion db queries
}