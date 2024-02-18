using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Features.Person.Interface;
using Cashrewards3API.Features.Person.Model;
using System.Linq;
using System.Threading.Tasks;
using NotFoundException = Cashrewards3API.Exceptions.NotFoundException;

namespace Cashrewards3API.Features.Person.Service
{
    using Cashrewards3API.Common.Events;
    using Cashrewards3API.Common.Services.Interfaces;
    using Cashrewards3API.Common.Services.Model;
    using Cashrewards3API.Enum;

    public class PersonService : IPerson
    {
        private readonly IMapper _mapper;
        private readonly IRepository _repository;
        private readonly IMessage _messageService;
        private readonly IDateTimeProvider _dateProvider;
        private readonly IReadOnlyRepository _readOnlyRepository;

        public PersonService(
            IMapper mapper,
            IRepository repository,
            IMessage messageService,
            IDateTimeProvider dateProvider,
            IReadOnlyRepository readOnlyRepository
            )
        {
            _mapper = mapper;
            _repository = repository;
            _messageService = messageService;
            _readOnlyRepository = readOnlyRepository;
            _dateProvider = dateProvider;
        }

        private async Task UpdatePersonOnDb(PersonModel personContext)
        {
            Person personDb = _mapper.Map<Person>(personContext, opts =>
            {
                opts.Items[Constants.Mapper.DateTimeUTC] = _dateProvider.UtcNow;
            });

            var updateQueryString = @"UPDATE [dbo].[Person]
                                                SET
                                                PremiumStatus = @PremiumStatus,
                                                UpdatedDateUTC = @UpdatedDateUTC
                                                WHERE CognitoId = @CognitoId";

            await _repository.Execute(updateQueryString, personDb);
        }

        private async Task UpdatePersonbyPersonIdOnDb(PersonModel personContext)
        {
            Person personDb = _mapper.Map<Person>(personContext, opts =>
            {
                opts.Items[Constants.Mapper.DateTimeUTC] = _dateProvider.UtcNow;
            });

            var updateQueryString = @"UPDATE [dbo].[Person]
                                                SET
                                                PremiumStatus = @PremiumStatus,
                                                UpdatedDateUTC = @UpdatedDateUTC,
                                                CognitoId = @CognitoId
                                                WHERE PersonId = @PersonId";

            await _repository.ExecuteAsyncWithRetry(updateQueryString, personDb);
        }

        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <param name="cognitoId">The cognito identifier.</param>
        /// <returns></returns>
        public async Task<PersonModel> GetPerson(string cognitoId)
        {
            return _mapper.Map<PersonModel>(await GetPersonFromDb(cognitoId));
        }

        /// <summary>
        /// Gets the person by Cognito Id
        /// </summary>
        /// <param name="cognitoId">The cognito identifier.</param>
        /// <returns></returns>
        private async Task<Person> GetPersonFromDb(string cognitoId)
        {
            var getQuery = @"SELECT [PersonId]
                                  ,[CognitoId]
                                  ,[PremiumStatus]
                                  ,[OriginationSource]
                                  ,[CreatedDateUTC]
                                  ,[UpdatedDateUTC] FROM [dbo].[Person]
                                    WHERE CognitoId = @CognitoId;
                                    SELECT m.* from CognitoMember cm
                                    INNER JOIN Member m on cm.MemberId = m.MemberId
                                    where cm.CognitoId=@CognitoId;";

            var response = await _repository.QueryTwoTablesAsync<Person, Member.Model.Member>(getQuery, new { cognitoId });
            Person personDb = response.Item1;
            if (personDb != null && response.Item2.Any())
                personDb.Members = response.Item2;
            return personDb;
        }

        private async Task<Person> GetPersonByIdFromDb(int personId)
        {
            var getQuery = @"SELECT [PersonId]
                                  ,[CognitoId]
                                  ,[PremiumStatus]
                                  ,[OriginationSource]
                                  ,[CreatedDateUTC]
                                  ,[UpdatedDateUTC] FROM [dbo].[Person]
                                    WHERE PersonId = @personId;
                                    SELECT m.* from CognitoMember cm
                                    INNER JOIN Member m on cm.MemberId = m.MemberId
                                    where cm.PersonId=@personId;";

            var response = await _repository.QueryTwoTablesAsync<Person, Member.Model.Member>(getQuery, new { personId });
            Person personDb = response.Item1;
            if (personDb != null && response.Item2.Any())
                personDb.Members = response.Item2;
            return personDb;
        }

        /// <summary>
        /// Updates the PersonPremiumStatusHistory and adds a new record with the new status
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        private async Task UpdatePersonPremiumHistory(PersonModel person)
        {
            var personPremiumHistory = _mapper.Map<PersonPremiumStatusHistory>(person, opts =>
            {
                opts.Items[Constants.Mapper.DateTimeUTC] = _dateProvider.UtcNow;
            });

            var queryUpdate = @"UPDATE PersonPremiumStatusHistory
            SET EndedAtUTC = @EndedAtUTC, UpdatedDateUTC = @UpdatedDateUTC
            WHERE PersonId = @PersonId AND EndedAtUTC IS NULL;";

            var queryInsert = @"INSERT INTO [dbo].[PersonPremiumStatusHistory]
           ([PersonId],[PremiumStatus],[ClientId],[StartedAtUTC],[EndedAtUTC],[CreatedDateUTC],[UpdatedDateUTC])
            VALUES (@PersonId,@PremiumStatus,@clientId,@StartedAtUTC,null,@CreatedDateUTC,null);";

            var query = string.Concat(queryUpdate, queryInsert);

            await _repository.ExecuteAsyncWithRetry(query, personPremiumHistory);
        }

        public async Task UpdatePerson(PersonModel person)
        {
            PremiumStatusEnum premiumStatus = person.PremiumStatus;
            Person personDb = await GetPersonFromDb(person.CognitoId.ToString());

            if (personDb == null)
                throw new NotFoundException($"The person with Cognito Id {person.CognitoId} could not be found");

            await UpdatePersonOnDb(person);
         
            person = _mapper.Map(personDb, person);
            person.PremiumStatus = premiumStatus;
            if (person.PremiumStatus != personDb.PremiumStatus)
            {
                await _messageService.UpdatedPremiumMemberProperty(_mapper.Map<MemberPremiumUpdateProperty>(person));
                await _messageService.UpdatePremiumMemberEvent(_mapper.Map<MemberPremiumUpdateEvent>(person), person.PremiumStatus);
                await UpdatePersonPremiumHistory(person);
            }
        }

        public async Task UpdatePersonById(PersonModel personContext)
        {
            await UpdatePersonbyPersonIdOnDb(personContext);
        }

        public async Task<PersonModel> GetPersonById(int personId)
        {
            return _mapper.Map<PersonModel>(await GetPersonByIdFromDb(personId));
        }

        public async Task<int?> GetMemberIdFromPersonIdAndClientId(int personId, int clientId)
        {
            string getMemberIdFromPersonIdAndClientIdQuery = @"
                SELECT TOP 1 MemberId as MemberId FROM Member JOIN Person on Member.PersonId = Person.PersonId
                    WHERE Member.PersonId = @PersonId
                        AND ClientId = @ClientId;
            ";

            return (await _readOnlyRepository.Query<int?>(getMemberIdFromPersonIdAndClientIdQuery, new
            {
                PersonId = personId,
                ClientId = clientId,
            })).FirstOrDefault();
        }
    }
}