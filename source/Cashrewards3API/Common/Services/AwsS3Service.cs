using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System.IO;
using System.Threading.Tasks;

namespace Cashrewards3API.Common.Services
{
    public interface IAwsS3Service
    {
        Task<string> ReadAmazonS3Data(string fileLocation, string bucket);
    }

    public class AwsS3Service : IAwsS3Service
    {
        /// <summary>
        /// Read a file from Amazon S3 storage, using default authorization
        /// </summary>
        /// <param name="fileLocation">file location</param>
        /// <param name="bucket">bucket</param>
        /// <exception cref="AmazonS3Exception">thrown if any details are incorrect</exception>
        /// <returns>string content of the file</returns>
        public async Task<string> ReadAmazonS3Data(string fileLocation, string bucket)
        {
            using (GetObjectResponse response = await ReadAmazonS3(fileLocation, bucket))
            using (Stream responseStream = response.ResponseStream)
            using (StreamReader reader = new StreamReader(responseStream))
            {
                var responseBody = reader.ReadToEnd();
                return responseBody;
            }
        }

        /// <summary>
        /// Read a file from Amazon S3 storage, using default authorization
        /// </summary>
        /// <param name="fileLocation">file location</param>
        /// <param name="bucket">bucket</param>
        /// <exception cref="AmazonS3Exception">thrown if any details are incorrect</exception>
        /// <returns>GetObjectResponse with the data stream</returns>
        private async Task<GetObjectResponse> ReadAmazonS3(string fileLocation, string bucket)
        {
            var config = new AmazonS3Config { RegionEndpoint = RegionEndpoint.APSoutheast2 };

            using (var s3Client = new AmazonS3Client(config))
            {
                var request = new GetObjectRequest
                {
                    BucketName = bucket,
                    Key = fileLocation
                };
                var objectresponse = await s3Client.GetObjectAsync(request);
                return objectresponse;
            }
        }
    }
}
