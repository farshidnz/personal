using Cashrewards3API.Features.Person.Model;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Person.Interface
{
    public interface IPerson
    {
        Task UpdatePerson(PersonModel person);

        Task<PersonModel> GetPerson(string cognitoId);

        Task<PersonModel> GetPersonById(int personId);
        Task UpdatePersonById(PersonModel person);

        Task<int?> GetMemberIdFromPersonIdAndClientId(int personId, int clientId);
    }
}