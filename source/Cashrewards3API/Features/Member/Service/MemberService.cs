using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Common.Dto;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Common.Services.Interfaces;
using Cashrewards3API.Exceptions;
using Cashrewards3API.Features.Member.Dto;
using Cashrewards3API.Features.Member.Interface;
using Cashrewards3API.Features.Member.Repository;
using Cashrewards3API.Features.Member.Request.UpdateCognitoMember;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Member.Service
{
    using Cashrewards3API.Common.Model;
    using Cashrewards3API.Common.Services.Model;
    using Cashrewards3API.Common.Utils;
    using Cashrewards3API.Features.Member.Request.SignInMember;
    using Cashrewards3API.Features.Member.Models;
    using Cashrewards3API.Features.Person.Interface;
    using Cashrewards3API.Features.Person.Model;
    using Member.Model;

    public class MemberService : IMemberService
    {
        private readonly IRepository _repository;
        private readonly IReadOnlyRepository _readOnlyRepository;
        private readonly IMapper _mapper;
        private readonly IEncryption _cryptor;

        private readonly IPerson _personService;
        private readonly IMemberRepository _memberRepository;
        private readonly ICacheKey _cacheKey;
        private readonly IRedisUtil _redisUtil;
        private readonly CacheConfig _cacheConfig;
        private readonly ITokenService _tokenService;

        private readonly IDateTimeProvider _dateTimeProvider;

        public MemberService(
            IRepository repository,
            IReadOnlyRepository readOnlyRepository,
            IMapper mapper,
            IEncryption cryptor,
            IPerson personService,
            IMemberRepository memberRepository,
            ICacheKey cacheKey,
            IRedisUtil redisUtil,
            CacheConfig cacheConfig,
            ITokenService tokenService,
            IDateTimeProvider dateTimeProvider)
        {
            _repository = repository;
            _readOnlyRepository = readOnlyRepository;
            _mapper = mapper;
            _cryptor = cryptor;
            _personService = personService;
            _memberRepository = memberRepository;
            _cacheKey = cacheKey;
            _redisUtil = redisUtil;
            _cacheConfig = cacheConfig;
            _tokenService = tokenService;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<IEnumerable<MemberDto>> GetAssocicatedMembersByMemberIdAsync(int memberId)
        {
            IEnumerable<MemberModel> members;

            var assoictedMemberIds = (List<int>)await _memberRepository.GetAssociatedMemberIds(memberId);

            if (!assoictedMemberIds.Any())
                assoictedMemberIds.Add(memberId);

            members = await _memberRepository.GetAssocicatedMembersByMemberId(assoictedMemberIds);
            if (!members.Any())
                throw new NotFoundException($"MemberId {memberId} does not exists");

            return  _mapper.Map<IEnumerable<MemberDto>>(members);
        }

        public async Task<MemberDto> GetMemberById(int memberId)
        {
            var queryString = @"SELECT mem.*, cogmem.CognitoPoolId, cogmem.CognitoId CognitoIdString, p.PremiumStatus, cogmem.PersonId, p.OriginationSource
                                FROM dbo.Member mem
                                LEFT JOIN dbo.CognitoMember cogmem on mem.MemberId = cogmem.MemberId
                                LEFT OUTER JOIN dbo.Person p on cogmem.PersonId = p.PersonId
                                WHERE mem.MemberId = @MemberId";

            var member = await _readOnlyRepository.QueryFirstOrDefault<MemberModel>(queryString, new
            {
                MemberId = memberId,
            });
            return member != null ? _mapper.Map<MemberDto>(member) : null;
        }

        public async Task<MemberEftposTransformer> GetMemberByIdForEftposTransformer(int memberId)
        {
            return (await _memberRepository.GetMemberByIdForEftposTransformer(memberId)).FirstOrDefault();
        }

        public async Task<MemberModel> GetMemberByClientIdAndMemberId(int clientId, int memberId)
        {
            var queryString = @"SELECT *
                                FROM dbo.Member
                                WHERE MemberId = @MemberId AND ClientId = @ClientId";

            return await _readOnlyRepository.QueryFirstOrDefault<MemberModel>(queryString, new
            {
                MemberId = memberId,
                ClientId = clientId
            });
        }

        public async Task<MemberModel> GetMemberByCognitoId(int clientId, string memberCognitoId)
        {
            var queryString = @"SELECT mem.*, cogmem.CognitoPoolId, cogmem.CognitoId CognitoIdString, p.PremiumStatus, cogmem.PersonId, p.OriginationSource
                                FROM dbo.Member mem
                                JOIN dbo.CognitoMember cogmem on mem.MemberId = cogmem.MemberId
                                LEFT OUTER JOIN dbo.Person p on cogmem.PersonId = p.PersonId
                                WHERE cogmem.CognitoId = @MemberCognitoId AND mem.ClientId = @ClientId";

            // using DB writer here as replication latency causing issues.
            var member = await _repository.QueryFirstOrDefault<MemberModel>(queryString, new
            {
                MemberCognitoId = memberCognitoId,
                ClientId = clientId
            });
            return member;
        }

        public async Task<SignInMemberInternalResponse> SignInMember(SignInMemberInternalRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return new SignInMemberInternalResponse("Email/Password required.");
            }

            var member = await _memberRepository.GetMemberInternalModelByEmailAndClientId(request.Email, Constants.Clients.CashRewards);
            if (member == null)
            {
                return new SignInMemberInternalResponse("Member not found.");
            }

            if (!_cryptor.VerifyStringWithSalt(request.Password, member.SaltKey, member.UserPassword))
            {
                return new SignInMemberInternalResponse("Invalid credentials.");
            }

            return new SignInMemberInternalResponse(_mapper.Map<SignInMemberInternalResponseMember>(member));
        }

        public async Task<MemberDto> GetMemberByClientIdAndEmail(int clientId, string email)
        {
            var query = @"SELECT *
                                FROM dbo.Member
                                WHERE Email = @Email AND ClientId=@ClientId";

            var member = await _readOnlyRepository.QueryFirstOrDefault<MemberModel>(query, new
            {
                Email = email,
                ClientId = clientId
            });

            if (member == null)
                return null;

            return ConvertToDto(member);
        }

        public async Task<List<MemberDto>> GetMembersByEmail(string email)
        {
            var query = @"SELECT *
                                FROM dbo.Member
                                WHERE Email = @Email";

            var member = await _readOnlyRepository.Query<MemberDto>(query, new
            {
                Email = email
            });

            return member.ToList();
        }

        private MemberDto ConvertToDto(MemberModel memberModel)
            => new()
            {
                MemberId = memberModel.MemberId,
                FirstName = memberModel.FirstName,
                LastName = memberModel.LastName,
                MemberNewId = memberModel.MemberNewId,
                AccessCode = memberModel.AccessCode,
                Email = memberModel.Email,
                ClientId = memberModel.ClientId,
                DateJoined = memberModel.DateJoined,
                OriginationSource = memberModel.OriginationSource,
                PostCode = memberModel.PostCode,
                Mobile = memberModel.Mobile,
                CampaignId = memberModel.CampaignId
            };

        public async Task<MemberModel> CreateCognitoMember(MemberModel cognitoMember)
        {
            Guid requestCognitoId = cognitoMember.CognitoId;

            List<MemberModel> existingMembers = (await GetMemberModel(cognitoMember)).ToList();
            if (existingMembers != null && existingMembers.Any() && existingMembers.Exists(m => m.ClientId == cognitoMember.ClientId)
                    && existingMembers.All(m => m.CognitoId == cognitoMember.CognitoId) && existingMembers.All(m => m.PersonId == existingMembers.First().PersonId))
            {
                cognitoMember.MemberInfo = existingMembers.ConvertAll(select => new MemberInfoDto() { ClientId = select.ClientId, MemberId = select.MemberId, MemberNewId = select.MemberNewId });
                await UpdateMemberLastLogon(cognitoMember);
                return cognitoMember;
            }

            if (cognitoMember.ClientId != Constants.Clients.CashRewards)
            {
                await CreateClientMember(cognitoMember, false);
            }

            cognitoMember = await CreateClientMember(cognitoMember, true);
            PersonModel person = null;
            if (cognitoMember.PersonId.HasValue)
                person = await _personService.GetPersonById((int)cognitoMember?.PersonId);

            if (person != null)
            {
                if (!string.Equals(person?.CognitoId, requestCognitoId))
                {
                    person.CognitoId = requestCognitoId;
                    await _personService.UpdatePersonById(_mapper.Map<PersonModel>(person));
                }
            }
            else
                person = _mapper.Map<PersonModel>(await CreatePerson(cognitoMember));

            cognitoMember.PersonId = person.PersonId;
            cognitoMember.OriginationSource = person.OriginationSource;

            await UpdateCognitoMemberTable(cognitoMember,
                (string.Equals(requestCognitoId.ToString(), cognitoMember.CognitoId.ToString()) ? null : requestCognitoId.ToString()));
            await UpdateMemberWithPersonId(cognitoMember);
            cognitoMember.CognitoId = requestCognitoId;
            return cognitoMember;
        }

        private async Task<Person> CreatePerson(MemberModel member)
        {
            return await InsertMemberInPersonDb(member);
        }

        private async Task<Person> InsertMemberInPersonDb(MemberModel member)
        {
            Person personDb = _mapper.Map<Person>(member);

            const string insertPerson = @"INSERT INTO [Person] (CognitoId,PremiumStatus,OriginationSource, CreatedDateUTC,UpdatedDateUTC)
                                                    VALUES (@CognitoId,@PremiumStatus,@OriginationSource, @CreatedDateUTC,@UpdatedDateUTC);
                                                    SELECT CAST(SCOPE_IDENTITY() as int);";

            personDb.PersonId = await _repository.QueryFirstOrDefault<int>(insertPerson, personDb);
            return personDb;
        }


        private async Task<IEnumerable<MemberModel>> GetMemberModel(MemberModel model)
        {
            var query = @"SELECT TOP 10
                            M.MemberId,
                            M.ClientId,
                            P.CognitoId,
                            M.Email,
                            M.MemberNewId,
                            M.PersonId,
                            CM.PersonId [CognitoMemberPersonId]
                            from Member M 
                            INNER join CognitoMember CM on M.MemberId = CM.MemberId
                            INNER JOIN Person P on P.PersonId= M.PersonId
                            where m.Email = @email";

            return await _readOnlyRepository.QueryAsync<MemberModel>(query, new
            {
                email = model.Email
            });

        }

        /// <summary>
        /// Updates the member last logon and LastLogonUTC values.
        /// </summary>
        /// <param name="model">The model.</param>
        private async Task UpdateMemberLastLogon(MemberModel model)
        {
            var command = @"UPDATE Member 
                            set LastLogon = @LastLogon,
                                LastLogonUtc = @LastLogonUtc                       
                            where MemberId in @memberId";

            await _repository.Execute(command, new
            {
                memberId = model.MemberInfo.Select(mem => mem.MemberId).ToArray(),
                LastLogon = _dateTimeProvider.Now,
                LastLogonUtc = _dateTimeProvider.UtcNow
            }) ;            
        }

        private async Task<MemberModel> CreateClientMember(MemberModel member, bool isCashrewards)
        {
            MemberDto clientMember = await GetMemberByClientIdAndEmail(isCashrewards ? Constants.Clients.CashRewards : member.ClientId, member.Email);

            if (clientMember == null)
            {
                member.ClientId = isCashrewards ? Constants.Clients.CashRewards : member.ClientId;
                member = await SaveMember(member);
            }
            else
                member = _mapper.Map(clientMember, member);

            member.MemberInfo.Add(new MemberInfoDto()
            {
                ClientId = clientMember == null ? member.ClientId : clientMember.ClientId,
                MemberNewId = clientMember == null ? member.MemberNewId : clientMember.MemberNewId,
                MemberId = clientMember == null ? member.MemberId : clientMember.MemberId
            });

            CognitoMemberModel cognitoMapping = await GetCognitoMemberByMemberId(clientMember != null ? clientMember.MemberId : member.MemberId);

            if (cognitoMapping == null)
                cognitoMapping = await InsertCognitoMemberIntoDb(_mapper.Map<CognitoMemberModel>(member));

            return _mapper.Map(cognitoMapping, member);
        }

        public async Task<CognitoMemberModel> InsertCognitoMemberIntoDb(CognitoMemberModel cognitoMember)
        {
            const string insertCognitoMember = @"INSERT INTO [CognitoMember] (CognitoId,MemberId,CognitopoolId, MemberNewId,CreatedAt,Status)
                                                    VALUES (@CognitoId,@MemberId,@CognitopoolId, @MemberNewId,@CreatedAt,@Status);
                                                    SELECT CAST(SCOPE_IDENTITY() as int);";

            await _repository.ExecuteAsyncWithRetry(insertCognitoMember, cognitoMember);
            return cognitoMember;
        }

        private async Task<MemberModel> SaveMember(MemberModel member)
        {
            MemberPassword memberPassword = CreateMemberPassword(member, member.Password);

            return _mapper.Map(await CreateMemberIntoDb(memberPassword, member), member);
        }

        private async Task<Member> CreateMemberIntoDb(MemberPassword password, MemberModel member)
        {
            Member memberDb = _mapper.Map<Member>(member);
            memberDb = _mapper.Map(password, memberDb);

            const string insertMemberQuery =
             @"INSERT INTO [Member]
                    (ClientId, Status, DateJoined, ActivateBy, FirstName, LastName,
                    PostCode, Mobile, ReceiveNewsLetter, Email, CookieIpAddress, AccessCode, UserPassword, SaltKey,
                    HashedEmail, HashedMobile, MemberNewId, DateOfBirth, CampaignId, Source,MobileSHA256,KycStatusId,ClickWindowActive,PopUpActive
                ,IsValidated,IsResetPassword,RequiredLogin,IsAvailable,IsRisky,InstallNotifier,SmsConsent,CommsPromptShownCount, FacebookUsername)
                VALUES
                    (@ClientId, @Status, @DateJoined, @ActivateBy, @FirstName, @LastName,
                    @PostCode,@Mobile , @ReceiveNewsLetter, @Email, @CookieIpAddress, @AccessCode, @UserPassword, @SaltKey,
                    @HashedEmail, @HashedMobile, @MemberNewId, @DateofBirth, @CampaignId, @Source,@MobileSHA256,1,@ClickWindowActive,@PopUpActive
                    ,@IsValidated,@IsResetPassword,@RequiredLogin,@IsAvailable,@IsRisky,@InstallNotifier,@SmsConsent,@CommsPromptShownCount, @FacebookUsername);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            var dynamicParameters = AddMemberParameters(memberDb);

            var insertResult = await _repository.QueryAsyncWithRetry<int>(insertMemberQuery, dynamicParameters);

            memberDb.MemberId = insertResult.First();
            return memberDb;
        }

        private static DynamicParameters AddMemberParameters(Member memberDb)
        {
            var dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("PostCode", memberDb.PostCode, DbType.String, ParameterDirection.Input, size: 50);
            dynamicParameters.Add("Mobile", memberDb.Mobile, DbType.String, ParameterDirection.Input, size: 50);
            dynamicParameters.Add("ClientId", memberDb.ClientId);
            dynamicParameters.Add("Status", memberDb.Status);
            dynamicParameters.Add("DateJoined", memberDb.DateJoined);
            dynamicParameters.Add("ActivateBy", memberDb.ActivateBy);
            dynamicParameters.Add("FirstName", memberDb.FirstName);
            dynamicParameters.Add("LastName", memberDb.LastName);
            dynamicParameters.Add("ReceiveNewsLetter", memberDb.ReceiveNewsLetter);
            dynamicParameters.Add("Email", memberDb.Email);
            dynamicParameters.Add("CookieIpAddress", memberDb.CookieIpAddress);
            dynamicParameters.Add("AccessCode", memberDb.AccessCode);
            dynamicParameters.Add("UserPassword", memberDb.UserPassword);
            dynamicParameters.Add("SaltKey", memberDb.SaltKey);
            dynamicParameters.Add("HashedEmail", memberDb.HashedEmail);
            dynamicParameters.Add("HashedMobile", memberDb.HashedMobile);
            dynamicParameters.Add("MemberNewId", memberDb.MemberNewId);
            dynamicParameters.Add("DateOfBirth", memberDb.DateofBirth, DbType.DateTime2, ParameterDirection.Input, precision: 7);
            dynamicParameters.Add("CampaignId", memberDb.CampaignId);
            dynamicParameters.Add("Source", memberDb.Source);
            dynamicParameters.Add("MobileSHA256", memberDb.MobileSHA256);
            dynamicParameters.Add("KycStatusId", memberDb.KycStatusId);
            dynamicParameters.Add("ClickWindowActive", memberDb.ClickWindowActive);
            dynamicParameters.Add("PopUpActive", memberDb.PopUpActive);
            dynamicParameters.Add("IsValidated", memberDb.IsValidated);
            dynamicParameters.Add("IsResetPassword", memberDb.IsResetPassword);
            dynamicParameters.Add("RequiredLogin", memberDb.RequiredLogin);
            dynamicParameters.Add("IsAvailable", memberDb.IsAvailable);
            dynamicParameters.Add("IsRisky", memberDb.IsRisky);
            dynamicParameters.Add("InstallNotifier", memberDb.InstallNotifier);
            dynamicParameters.Add("SmsConsent", memberDb.SmsConsent);
            dynamicParameters.Add("CommsPromptShownCount", memberDb.CommsPromptShownCount);
            dynamicParameters.Add("FacebookUsername", memberDb.FacebookUsername);
            return dynamicParameters;
        }

        /// <summary>
        /// Creates the member password.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="plainPassword">The plain password.</param>
        /// <returns></returns>
        private MemberPassword CreateMemberPassword(MemberModel member, string plainPassword)
        {
            var memberPassword = new MemberPassword();

            memberPassword.SaltKey = _cryptor.GenerateSaltKey(20);
            memberPassword.UserPassword = _cryptor.EncryptWithSalting(plainPassword, memberPassword.SaltKey);
            //- new
            if (!string.IsNullOrEmpty(member.Email))
            {
                memberPassword.HashedEmail = _cryptor.EncryptWithSalting(member.Email, memberPassword.SaltKey).Replace("+", "S1U3L9P").Replace("/", "S1L3A9S").Replace(@"\", "F7L4S0H");
            }

            //- new
            if (!string.IsNullOrEmpty(member.Mobile))
            {
                memberPassword.HashedMobile = _cryptor.EncryptWithSalting(member.Mobile, memberPassword.SaltKey).Replace("+", "S1U3L9P").Replace("/", "S1L3A9S").Replace(@"\", "F7L4S0H");
            }

            return memberPassword;
        }

        /// <summary>
        /// Gets the cognito member by member identifier cognito identifier cognito pool identifier.
        /// </summary>
        /// <param name="memberId">Member ID.</param>
        /// <param name="cognitoId">Cognito ID</param>
        /// <param name="cognitoPoolId">CognitoPoolID</param>
        /// <returns></returns>
        public async Task<CognitoMemberModel> GetCognitoMemberByMemberIdCognitoIdCognitoPoolId(int memberId, Guid cognitoId, string cognitoPoolId)
        {
            var query = @"SELECT *
                                FROM dbo.CognitoMember
                                WHERE MemberId = @MemberId AND CognitoId=@CognitoId AND CognitopoolId=@CognitoPoolId";

            var cognitoMember = await _readOnlyRepository.QueryFirstOrDefault<CognitoMemberModel>(query, new
            {
                memberId,
                cognitoId = cognitoId.ToString(),
                cognitoPoolId
            });

            return cognitoMember;
        }

        /// <summary>
        /// Gets the cognito member by member identifier.
        /// </summary>
        /// <param name="memberId">The member identifier.</param>
        /// <returns></returns>
        public async Task<CognitoMemberModel> GetCognitoMemberByMemberId(int memberId)
        {
            var query = @"SELECT [MemberId],[CognitoId],[CognitoPoolId], [PersonId]
                                FROM dbo.CognitoMember
                                WHERE MemberId = @MemberId";

            var cognitoMember = await _readOnlyRepository.QueryFirstOrDefault<CognitoMemberModel>(query, new
            {
                memberId,
            });

            return cognitoMember;
        }

        private async Task UpdateCognitoMemberWithPersonId(MemberModel member)
        {
            const string updateCognitoMember = @"UPDATE [dbo].[CognitoMember]
                                                 set PersonId = @PersonId
                                                        UpdatedAt = @UpdatedAt
                                                  WHERE CognitoId = @CognitoId";

            await _repository.Execute(updateCognitoMember, new { member.PersonId, CognitoId = member.CognitoId.ToString(), UpdatedAt = DateTime.Now });
        }

        /// <summary>
        /// Updates the cognito member table.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="newCognitoId">The new cognito identifier.</param>
        /// <returns></returns>
        private async Task UpdateCognitoMemberTable(MemberModel model, string newCognitoId = null)
        {
            const string updateCognitoMember = @"UPDATE [dbo].[CognitoMember]
                                                 set PersonId = @PersonId,
                                                    CognitoId = @NewCognitoId,
                                                      UpdatedAt = @UpdatedAt
                                                  WHERE CognitoId = @CognitoId";

            await _repository.ExecuteAsyncWithRetry(updateCognitoMember, new
            {
                model.PersonId,
                NewCognitoId = (newCognitoId ?? model.CognitoId.ToString()),
                CognitoId = model.CognitoId.ToString(),
                UpdatedAt = DateTime.Now
            });
        }

        private async Task UpdateMemberWithPersonId(MemberModel member)
        {
            const string updateMember = @"UPDATE [dbo].[Member]
                                                 SET PersonId = @PersonId,
                                                    LastLogon = @LastLogon,
                                                    LastLogonUtc = @LastLogonUtc   
                                                 WHERE MemberId in (Select MemberId from CognitoMember Where CognitoId = @CognitoId)";

            await _repository.ExecuteAsyncWithRetry(updateMember, new { member.CognitoId,member.PersonId, LastLogon  = _dateTimeProvider.Now, LastLogonUtc = _dateTimeProvider.UtcNow});
        }

        public async Task<UpdateFacebookUsernameDto> UpdateFacebookUsername(UpdateFacebookUsernameRequest request)
        {
            var member = await _memberRepository.GetMemberModelByEmailAndClientId(request.FacebookUsername, Constants.Clients.CashRewards);
            if (member == null)
                return new UpdateFacebookUsernameDto(false);

            ValidateMemberForFacebookUsername(member, request.FacebookUsername);
            var facebookMember = await _memberRepository.GetMemberModelByFacebookUsernameAndClientId(request.FacebookUsername, Constants.Clients.CashRewards);
            if (facebookMember == null)
                await _memberRepository.UpdateFacebookUsernameIntoDb(member.MemberId, request.FacebookUsername);

            return new UpdateFacebookUsernameDto(true);
        }

        private void ValidateMemberForFacebookUsername(MemberModel member, string facebookUsername)
        {
            if (!member.Email.Equals(facebookUsername, StringComparison.InvariantCultureIgnoreCase))
                throw new NotFoundException("Invalid facebookUsername value for member");
        }

        /// <summary>
        /// Gets the tr authentication token.
        /// </summary>
        /// <param name="memberContextModel">The member context model.</param>
        /// <returns></returns>
        public async Task<string> GetTRAuthToken(MemberContextModel memberContextModel)
        {
            string firstName = memberContextModel.FirstName;
            string lastName = memberContextModel.LastName;
            string email = memberContextModel.Email;

            string fullName = (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName)) ? email : $"{firstName} {lastName}";
            
            string key = _cacheKey.GetTRAuthTokenKey(fullName, email);
            return await _redisUtil.GetDataAsync(key,
                () => GetAuthTokenFromTrueRewards(fullName, email),
                _cacheConfig.MerchantDataExpiry);

        }


        private async Task<string> GetAuthTokenFromTrueRewards(string name, string email)
        {
            AuthnRequestContext requestContext = new AuthnRequestContext() {
                Email = email,
                FullName = name      
            };                        
            TokenContext token =  await _tokenService.GetToken(requestContext);            
            return token?.AuthToken;            
        }

        public async Task<EmailMemberDto> GetMemberByEmail(string email)
        {
            var queryString = @"SELECT MemberId, ClientId, Status, FirstName,
                                        LastName, PostCode, Mobile, Email, 
                                        AccessCode, IsValidated, DateJoined, MemberNewId, 
                                        CampaignId, Source, ReceiveNewsLetter
                                FROM dbo.Member
                                WHERE Email = @Email";

            var member = await _readOnlyRepository.QueryFirstOrDefault<MemberModel>(queryString, new
            {
                Email = email,
            });
            return member != null ? _mapper.Map<EmailMemberDto>(member) : null;
        }

        /// <summary>
        /// Members have one MemberNewId for each ClientId. Cr/Max/MME
        /// This endpoint will take a MemberNewId for a user return the MemberNewId for that user that matches the supplied ClientId
        /// eg; Max (ClientId: 1000034) MemberNewId to Cr (ClientId: 1000000) MemberNewId
        /// </summary>
        /// <param name="sourceMemberNewId">Known MemberNewId of a user</param>
        /// <param name="targetClientId">Desired MemberNewId matching this ClientId</param>
        /// <returns>MemberNewId for the Member with the supplied MemberNewId that matches the supplied ClientId</returns>
        public async Task<Guid?> MapMembersMemberNewIdToMemberNewIdWithClientId(string sourceMemberNewId, int targetClientId)
        {
            var query = @"SELECT T.[MemberNewId]
                                FROM dbo.Member T JOIN dbo.Member S ON S.PersonId = T.PersonId
                                WHERE S.MemberNewId = @SourceMemberNewId AND T.[ClientId] = @TargetClientId";

            var memberWithJustMemberNewId = await _readOnlyRepository.QueryFirstOrDefault<Member>(query, new
            {
                SourceMemberNewId = sourceMemberNewId,
                TargetClientId = targetClientId
            });

            return memberWithJustMemberNewId?.MemberNewId;
        }

    }

}