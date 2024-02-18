using Cashrewards3API.Common.Services.Interfaces;
using Moq;
using System;

namespace Cashrewards3API.Tests.Helpers
{
    public class IDateTimeProviderMock : Mock<IDateTimeProvider>
    {
        public IDateTimeProviderMock Setup<T>()
        {
            Setup(setup => setup.Now).Returns(DateTime.Now);
            Setup(setup => setup.UtcNow).Returns(DateTime.UtcNow);
            return this;
        }
    }
}