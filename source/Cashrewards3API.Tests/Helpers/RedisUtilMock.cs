using Cashrewards3API.Common.Utils;
using Moq;
using System;
using System.Threading.Tasks;

namespace Cashrewards3API.Tests.Helpers
{
    public class RedisUtilMock : Mock<IRedisUtil>
    {
        public RedisUtilMock Setup<T>() where T : class
        {
            Setup(r => r.GetDataAsync(It.IsAny<string>(), It.IsAny<Func<Task<T>>>(), It.IsAny<int>()))
                .Returns((string key, Func<Task<T>> action, int expiryTime) => action());

            Setup(r => r.GetDataAsyncWithEarlyRefresh(It.IsAny<string>(), It.IsAny<Func<Task<T>>>(), It.IsAny<int>()))
                .Returns((string key, Func<Task<T>> action, int expiryTime) => action());

            return this;
        }
    }
}
