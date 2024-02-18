using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Common.Services.Interfaces
{
    public interface IEncryption
    {
        byte[] Encrypt(byte[] inputValue);
        string Encrypt(string inputValue);
        string EncryptWithSalting(string inputString, byte[] saltBytes = null);
        string EncryptWithSalting(string inputString, string saltString);
        string DecryptWithSalting(string toDecrypt, string saltKey);
        bool VerifyHashWithSalt(string inputValue, string hashValue);
        bool VerifyStringWithSalt(string inputString, string saltString, string hashValue);
        string GenerateSaltKey(int size);
        string GenerateRandomPassword();
    }
}
