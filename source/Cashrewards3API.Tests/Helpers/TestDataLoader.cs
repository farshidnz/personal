using Newtonsoft.Json;
using System;
using System.IO;

namespace Cashrewards3API.Tests.Helpers
{
    public static class TestDataLoader
    {
        public static T Load<T>(string testDataFileName, JsonSerializerSettings settings = null) => JsonConvert.DeserializeObject<T>(File.ReadAllText(testDataFileName.Replace("\\", "/")), settings);

        public static T TryLoad<T>(string testDataFileName, JsonSerializerSettings settings = null)
        {
            try
            {
                return Load<T>(testDataFileName, settings);
            }
            catch (Exception x)
            {
                Console.WriteLine(x);
                return default;
            }
        }

        public static string Load(string testDataFileName) => File.ReadAllText(testDataFileName.Replace("\\", "/"));

        public static string TryLoad(string testDataFileName)
        {
            try
            {
                return Load(testDataFileName);
            }
            catch
            {
                return default;
            }
        }
    }
}
