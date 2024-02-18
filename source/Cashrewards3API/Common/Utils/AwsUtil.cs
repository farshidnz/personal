using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System.IO;
using System.Threading.Tasks;


namespace Cashrewards3API.Common.Utils
{
    public static class AwsUtil
    {
        /// <summary>
        /// Read a file from Amazon S3 storage
        /// </summary>
        /// <param name="fileLocation">file location</param>
        /// <param name="bucket">bucket</param>
        /// <param name="accessKey">access key</param>
        /// <param name="secretKey">secret key</param>
        /// <exception cref="AmazonS3Exception">thrown if any details are incorrect</exception>
        /// <returns>string content of the file</returns>
        public static async Task<string> ReadAmazonS3Data(string fileLocation, string bucket, string accessKey, string secretKey)
        {
            using (GetObjectResponse response = await ReadAmazonS3(fileLocation, bucket, accessKey, secretKey))
            using (Stream responseStream = response.ResponseStream)
            using (StreamReader reader = new StreamReader(responseStream))
            {
                var responseBody = reader.ReadToEnd();
                return responseBody;
            }
        }

        /// <summary>
        /// Read a file from Amazon S3 storage
        /// </summary>
        /// <param name="fileLocation">file location</param>
        /// <param name="bucket">bucket</param>
        /// <param name="accessKey">access key</param>
        /// <param name="secretKey">secret key</param>
        /// <exception cref="AmazonS3Exception">thrown if any details are incorrect</exception>
        /// <returns>GetObjectResponse with the data stream</returns>
        public static async Task<GetObjectResponse> ReadAmazonS3(string fileLocation, string bucket, string accessKey, string secretKey)
        {
            var config = new AmazonS3Config { RegionEndpoint = RegionEndpoint.APSoutheast2 };

            using (var s3Client = new AmazonS3Client(accessKey, secretKey, config))
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

        /// <summary>
        /// Read a file from Amazon S3 storage
        /// </summary>
        /// <param name="fileLocation">file location</param>
        /// <param name="bucket">bucket</param>
        /// <param name="accessKey">access key</param>
        /// <param name="secretKey">secret key</param>
        /// <exception cref="AmazonS3Exception">thrown if any details are incorrect</exception>
        /// <returns>string content of the file</returns>
        public static async Task<string> ReadAmazonS3DataAsync(string fileLocation, string bucket, string accessKey, string secretKey)
        {
            using (GetObjectResponse response = await ReadAmazonS3Async(fileLocation, bucket, accessKey, secretKey))
            using (Stream responseStream = response.ResponseStream)
            using (StreamReader reader = new StreamReader(responseStream))
            {
                var responseBody = reader.ReadToEnd();
                return responseBody;
            }
        }

        /// <summary>
        /// Read a file from Amazon S3 storage
        /// </summary>
        /// <param name="fileLocation">file location</param>
        /// <param name="bucket">bucket</param>
        /// <param name="accessKey">access key</param>
        /// <param name="secretKey">secret key</param>
        /// <exception cref="AmazonS3Exception">thrown if any details are incorrect</exception>
        /// <returns>GetObjectResponse with the data stream</returns>
        public static async Task<GetObjectResponse> ReadAmazonS3Async(string fileLocation, string bucket, string accessKey, string secretKey)
        {
            var config = new AmazonS3Config { RegionEndpoint = RegionEndpoint.APSoutheast2 };

            using (var s3Client = new AmazonS3Client(accessKey, secretKey, config))
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
