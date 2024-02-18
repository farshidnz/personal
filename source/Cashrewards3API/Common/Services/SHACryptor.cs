using Cashrewards3API.Common.Services.Interfaces;
using Cashrewards3API.Extensions;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Cashrewards3API.Common.Services
{
    public class SHACryptor : IEncryption
    {
        public byte[] Encrypt(byte[] inputValue)
        {
            SHA1CryptoServiceProvider cryptor = new SHA1CryptoServiceProvider();
            byte[] hash = cryptor.ComputeHash(inputValue);
            return hash;
        }

        public string Encrypt(string inputValue)
        {
            byte[] arrByte, arrHash;
            ASCIIEncoding asciiEncode = new ASCIIEncoding();
            arrByte = asciiEncode.GetBytes(inputValue);
            arrHash = Encrypt(arrByte);
            return Convert.ToBase64String(arrHash);
        }

        public string EncryptWithSalting(string inputValue, byte[] saltBytes = null)
        {
            if (saltBytes == null)
            {
                int minSaltSize = 4;
                int maxSaltSize = 8;

                Random random = new Random();
                int saltSize = random.Next(minSaltSize, maxSaltSize);
                saltBytes = new byte[saltSize - 1];
                RNGCryptoServiceProvider serviceProvider = new RNGCryptoServiceProvider();
                serviceProvider.GetNonZeroBytes(saltBytes);
            }

            byte[] plainTextBytes = Encoding.UTF8.GetBytes(inputValue);
            byte[] plainTextWithSaltBytes = new byte[plainTextBytes.Length + saltBytes.Length];

            for (int i = 0; i <= (plainTextBytes.Length - 1); i++)
                plainTextWithSaltBytes[i] = plainTextBytes[i];

            for (int i = 0; i <= (saltBytes.Length - 1); i++)
                plainTextWithSaltBytes[plainTextBytes.Length + i] = saltBytes[i];

            byte[] arrHash = Encrypt(plainTextWithSaltBytes);
            byte[] hashWithSaltBytes = new byte[arrHash.Length + saltBytes.Length];

            for (int i = 0; i <= arrHash.Length - 1; i++)
                hashWithSaltBytes[i] = arrHash[i];

            for (int i = 0; i <= saltBytes.Length - 1; i++)
                hashWithSaltBytes[arrHash.Length + i] = saltBytes[i];

            return Convert.ToBase64String(hashWithSaltBytes);
        }

        public bool VerifyHashWithSalt(string inputValue, string hashValue)
        {
            try
            {
                if (hashValue.Length > 1)
                {
                    byte[] hashWithSaltBytes = Convert.FromBase64String(hashValue);
                    int hashSizeInBits, hashSizeInBytes;

                    hashSizeInBits = 160;
                    hashSizeInBytes = hashSizeInBits / 8;

                    if (hashWithSaltBytes.Length < hashSizeInBytes)
                    {
                        return false;
                    }

                    byte[] saltBytes = new byte[hashWithSaltBytes.Length - hashSizeInBytes];

                    for (int i = 0; i <= saltBytes.Length - 1; i++)
                        saltBytes[i] = hashWithSaltBytes[hashSizeInBytes + i];

                    string expectedHashString = EncryptWithSalting(inputValue, saltBytes);

                    if (hashValue == expectedHashString)
                        return true;
                    else
                        return false;
                }
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }

        #region Encrypt/Verify With String

        public string EncryptWithSalting(string inputString, string saltString)
        {
            try
            {
                return GetSHA256Hash(inputString, saltString);
            }
            catch
            {
                return null;
            }
        }

        public string DecryptWithSalting(string toDecrypt, string saltKey)
        {
            string decrypted = string.Empty;
            try
            {
                byte[] data = System.Convert.FromBase64String(toDecrypt);
                byte[] rgbKey = ASCIIEncoding.ASCII.GetBytes(saltKey);
                byte[] rgbIV = ASCIIEncoding.ASCII.GetBytes(saltKey.ReverseString());

                //1024-bit decryption

                MemoryStream memoryStream = new MemoryStream(data.Length);
                DESCryptoServiceProvider desCryptoServiceProvider = new DESCryptoServiceProvider();
                ICryptoTransform x = desCryptoServiceProvider.CreateDecryptor(rgbKey, rgbIV);
                CryptoStream cryptoStream = new CryptoStream(memoryStream, x, CryptoStreamMode.Read);
                memoryStream.Write(data, 0, data.Length);
                memoryStream.Position = 0;
                decrypted = new StreamReader(cryptoStream).ReadToEnd();
                cryptoStream.Close();
                memoryStream.Dispose();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return decrypted;
        }

        public bool VerifyStringWithSalt(string inputString, string saltString, string hashValue)
        {
            try
            {
                //string encryptedString = this.EncryptWithSalting(inputString, saltString);
                //return (encryptedString == hashValue);

                return CompareByteArrays(GetSHA256Hash(inputString, saltString), hashValue);
            }
            catch
            {
                return false;
            }
        }

        #endregion Encrypt/Verify With String

        public string GenerateSaltKey(int size)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            return builder.ToString();
        }

        public string GenerateRandomPassword()
        {
            return this.RandomString(9);
        }

        public string GetSHA256Hash(string s, string t)
        {
            Byte[] salt = System.Text.Encoding.UTF8.GetBytes(t);
            Byte[] data = System.Text.Encoding.UTF8.GetBytes(s);

            byte[] plainTextWithSaltBytes = new byte[data.Length + salt.Length];
            for (int i = 0; i < data.Length; i++)
            {
                plainTextWithSaltBytes[i] = data[i];
            }
            for (int i = 0; i < salt.Length; i++)
            {
                plainTextWithSaltBytes[data.Length + i] = salt[i];
            }

            Byte[] hash = new SHA256CryptoServiceProvider().ComputeHash(plainTextWithSaltBytes);
            return Convert.ToBase64String(hash);
        }

        public string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            return builder.ToString();
        }

        public bool CompareByteArrays(string a, string b)
        {
            byte[] array1 = System.Text.Encoding.UTF8.GetBytes(a);
            byte[] array2 = System.Text.Encoding.UTF8.GetBytes(b);

            if (array1.Length != array2.Length)
            {
                return false;
            }

            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}