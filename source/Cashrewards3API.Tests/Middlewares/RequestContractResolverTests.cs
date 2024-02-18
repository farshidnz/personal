using Cashrewards3API.Middlewares;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Cashrewards3API.Tests.Middlewares
{
    public class RequestContractResolverTests
    {
        private class TestState
        {
            public RequestContractResolver RequestContractResolver { get; }

            public TestState(string acceptCase)
            {
                var httpContextAccessor = new Mock<IHttpContextAccessor>();
                var httpContext = new Mock<HttpContext>();
                var request = new Mock<HttpRequest>();
                var headers = new Mock<IHeaderDictionary>();
                headers.Setup(q => q["AcceptCase"]).Returns(acceptCase);
                request.Setup(r => r.Headers).Returns(headers.Object);
                httpContext.Setup(h => h.Request).Returns(request.Object);
                httpContextAccessor.Setup(a => a.HttpContext).Returns(httpContext.Object);
                RequestContractResolver = new RequestContractResolver(httpContextAccessor.Object);
            }
        }

        private class DataContract
        {
            public string FirstName { get; set; }
        }

        [TestCase(null, @"{""FirstName"":""Bob""}")]
        [TestCase("camel", @"{""firstName"":""Bob""}")]
        public void RequestContractResolver_ShouldSerializeBasedOnRequestedFormat(string acceptCase, string expectedJson)
        {
            var state = new TestState(acceptCase);
            var data = new DataContract { FirstName = "Bob" };

            var json = JsonConvert.SerializeObject(data, new JsonSerializerSettings
            {
                ContractResolver = state.RequestContractResolver
            });

            json.Should().Be(expectedJson);
        }
    }
}
