using Cashrewards3API.Common;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Enum;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.MemberClick
{

    public interface IWoolworthsEncryptionProvider
    {
        Task<WoolworthsEncryptionModel> GetWoolworthsEncryptionDetails(int clientId, int merchantId, int memberId);
    }

    public class WoolworthsEncryptionProvider : IWoolworthsEncryptionProvider
    {
        private readonly IReadOnlyRepository _readOnlyRepository;

        public WoolworthsEncryptionProvider(IReadOnlyRepository readOnlyRepository)
        {
            _readOnlyRepository = readOnlyRepository;
        }

        public async Task<WoolworthsEncryptionModel> GetWoolworthsEncryptionDetails(int clientId, int merchantId, int memberId)
        {
            string memberClientId = $"{memberId}-{clientId}";
            string clientKey = string.Empty;
            string siteReferenceId = string.Empty;
            string timeStampEncrypted = string.Empty;
            int clientKeyParameterTypeId = 0;
            int siteReferenceParameterTypeId = 0;

            switch (merchantId)
            {
                case Constants.Merchants.Woolworths:
                    clientKeyParameterTypeId = (int)ClientParameterTypeEnum.WoolworthsClientKey;
                    siteReferenceParameterTypeId = (int)ClientParameterTypeEnum.WoolworthsClientId;
                    break;
                case Constants.Merchants.Masters:
                    clientKeyParameterTypeId = (int)ClientParameterTypeEnum.MastersClientKey;
                    siteReferenceParameterTypeId = (int)ClientParameterTypeEnum.MastersClientId;
                    break;
                case Constants.Merchants.BigW:
                    clientKeyParameterTypeId = (int)ClientParameterTypeEnum.BigwClientKey;
                    siteReferenceParameterTypeId = (int)ClientParameterTypeEnum.BigwClientId;
                    break;
                default:
                    clientKeyParameterTypeId = (int)ClientParameterTypeEnum.WoolworthsClientKey;
                    siteReferenceParameterTypeId = (int)ClientParameterTypeEnum.WoolworthsClientId;
                    break;
            }

            ClientParameterModel clientKeyClientParameter = await GetClientParameterByParameterType(clientKeyParameterTypeId, clientId);
            if (clientKeyClientParameter != null)
            {
                clientKey = clientKeyClientParameter.ClientParameterValue;
            }

            ClientParameterModel siteReferenceIdClientParameter = await GetClientParameterByParameterType (siteReferenceParameterTypeId, clientId);
            if (siteReferenceIdClientParameter != null)
            {
                siteReferenceId = siteReferenceIdClientParameter.ClientParameterValue;
            }

            if (clientKey != string.Empty)
            {
                timeStampEncrypted = Encrypt(DateTime.UtcNow.ToString("MMM dd, yyyy HH:mm:ss tt"), clientKey);
            }

            return new WoolworthsEncryptionModel()
            {
                WwEncryptedClientMemberId = memberClientId,
                WwEncryptedSiteReferenceId = siteReferenceId,
                WwEncryptedTimeStamp = timeStampEncrypted
            };
        }

        /// <summary>
        /// Encrypts plaintext using AES 128bit key and a Chain Block Cipher and returns a base64 encoded string
        /// </summary>
        /// <param name="plainText">Plain text to encrypt</param>
        /// <param name="key">Secret key</param>
        /// <returns>Base64 encoded string</returns>
        private static string Encrypt(string plainText, string key)
        {
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(Encrypt(plainBytes, GetRijndaelManaged(key)));
        }

        private static byte[] Encrypt(byte[] plainBytes, RijndaelManaged rijndaelManaged)
        {
            return rijndaelManaged.CreateEncryptor()
                .TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        }

        private static RijndaelManaged GetRijndaelManaged(string secretKey)
        {
            var keyBytes = new byte[16];
            var secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);
            Array.Copy(secretKeyBytes, keyBytes, Math.Min(keyBytes.Length, secretKeyBytes.Length));
            return new RijndaelManaged
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                KeySize = 128,
                BlockSize = 128,
                Key = keyBytes,
                IV = keyBytes
            };
        }

        private async Task<ClientParameterModel> GetClientParameterByParameterType(int parameterTypeId, int clientId)
        {
            var queryString = @"SELECT * 
                                FROM dbo.ClientParameter 
                                WHERE ClientId = @ClientId AND ClientParameterTypeId = @ClientParameterTypeId";

            var clientParameterType = await _readOnlyRepository.QueryFirstOrDefault<ClientParameterModel>(queryString, new
            {
                ClientId = clientId,
                ClientParameterTypeId = parameterTypeId
            });

            return clientParameterType;
        }
    }
}
