using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Common.Services.Interfaces;
using Cashrewards3API.Features.Person.Model;
using Cashrewards3API.Features.Person.Request.UpdatePerson;
using Cashrewards3API.Features.Person.Service;
using Cashrewards3API.Mapper;
using Cashrewards3API.Tests.Helpers;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.AlwaysEncrypted.AzureKeyVaultProvider;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Cashrewards3API.Tests.Features.Person
{
    [TestFixture]
    public class PersonServiceTest
    {
        private IMapper mapper;

        [SetUp]
        public void SetUp()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<PersonProfile>());
            mapper = config.CreateMapper();
        }

        private UpdatePersonRequest request = new UpdatePersonRequest()
        {
            CognitoId = "8748db48-f3fe-4c06-92e8-5df0fa49b1c3",
            PremiumStatus = Enum.PremiumStatusEnum.OptOut
        };

        [Test]
        public void ValidateMapFromUpdatePersonRequest_To_PersonModel()
        {
            PersonModel person = mapper.Map<PersonModel>(request);

            person.CognitoId.Should().Be(request.CognitoId);
            person.PremiumStatus.Should().Be(request.PremiumStatus);
        }

        private void EnableColumnEncryption()
        {
            SqlConnection.RegisterColumnEncryptionKeyStoreProviders(new Dictionary<string, SqlColumnEncryptionKeyStoreProvider>
            {
                [SqlColumnEncryptionAzureKeyVaultProvider.ProviderName] = new SqlColumnEncryptionAzureKeyVaultProvider(async (string authority, string resource, string scope) =>
                    (await new AuthenticationContext(authority).AcquireTokenAsync(resource, new ClientCredential("AzureAADClientId", "AzureAADClientSecret"))).AccessToken
                )
            });
        }

        [Test]
        [Ignore("integration test")]
        public async Task GetPerson_ShouldReturnNull_GivenNoPersonExists()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile($"{Assembly.Load("Cashrewards3API").Folder()}/appsettings.Development.json", true)
                .Build();
            EnableColumnEncryption();
            var repository = new Repository(new ShopgoDBContext(new DbConfig { ShopgoDbContext = configuration["ConnectionStrings:ShopgoDbContext"] }, configuration));
            var personService = new PersonService(mapper, repository, Mock.Of<IMessage>(), Mock.Of<IDateTimeProvider>(), repository);

            var person = await personService.GetPerson(Guid.NewGuid().ToString());

            person.Should().BeNull();
        }
    }
}
