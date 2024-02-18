using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Common.Services.Interfaces;
using Cashrewards3API.Common.Utils;
using Cashrewards3API.Exceptions;
using Cashrewards3API.Features.Member.Interface;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Net.Http.Headers;
using Moq;
using NUnit.Framework;
using System;

namespace Cashrewards3API.Tests.Common.Services
{
    [TestFixture]
    [SetCulture("en-Au")]
    public class RequestContextTests
    {
        private readonly string _clientId = "659gae2lr4cgj4sllfovk85itt";
        private readonly string _cognitoUserId = "83dd376d-9648-4aab-a852-66ebdcd35c74";
        private readonly string JWT = "eyJraWQiOiJDd2dPMHVRWUZVQXNkU1U0Rk9mSUMxTzFtVHdiZTBPVFF2T0lHdkF6M25NPSIsImFsZyI6IlJTMjU2In0.eyJzdWIiOiJmNjQ4OTJiZi1hZmUzLTRjMDAtODVlOS03NWUwMmM5Y2MzZmIiLCJldmVudF9pZCI6ImUzMTMzZTZjLWE4OTQtNDgyZC04Y2M4LTIwM2FmNTM2NGY0YiIsInRva2VuX3VzZSI6ImFjY2VzcyIsInNjb3BlIjoiYXdzLmNvZ25pdG8uc2lnbmluLnVzZXIuYWRtaW4iLCJhdXRoX3RpbWUiOjE2MjE4MjEyNjUsImlzcyI6Imh0dHBzOlwvXC9jb2duaXRvLWlkcC5hcC1zb3V0aGVhc3QtMi5hbWF6b25hd3MuY29tXC9hcC1zb3V0aGVhc3QtMl85cTZUWGFpOTkiLCJleHAiOjE2MjE4MjQ4NjUsImlhdCI6MTYyMTgyMTI2NSwianRpIjoiNDE2MmM0MzQtZGQ5MC00YWRjLWFlZDUtZWVmYjA3MjIyYjQ5IiwiY2xpZW50X2lkIjoiNjU5Z2FlMmxyNGNnajRzbGxmb3ZrODVpdHQiLCJ1c2VybmFtZSI6IjgzZGQzNzZkLTk2NDgtNGFhYi1hODUyLTY2ZWJkY2QzNWM3NCJ9.TTWSpMZ75abbKtUwy12yFUCY62cgplqYZDy_LWMXnJMpIwF1HMWABf9Rj-MN5KhUvciH0hp9IHnsuhrzUW5ZBcuZktLgH4RQkAekwdZw4_f3bUbDXGG-mcp9qc_bc4y-byS9b0q2ngrSH5aDnG5El6zbvC2rlkPtqKrOIAzuGwSa7KsoIsGYRUWsxo_l3aLrR0CsnW_4gWnqz1wC-sxRp_m7AhN--4LWQG0ovagnaKQGkeS4Vs8Y4plNl74RtNyQlY-Dw5xmkzKaoVh5dzbY7yGhYIR_eOQRhzb_sacWqma8YMQ8PvxYoAWbbqwIE9hvpjgy6cNkb9UVeGeSTo5ECw";

        private readonly Mock<IHttpContextAccessor> httpContextAccessor = new();
        private readonly Mock<IClientService> clientService = new();
        private readonly Mock<IMemberService> memberService = new();
        private readonly Mock<IRedisUtil> redisUtil = new();
        private readonly Mock<IMemoryCache> memoryCache = new();
        private readonly Mock<IMapper> mapper = new();
        private readonly Mock<IPremiumService> premiumService = new();
        private readonly Mock<IDateTimeProvider> dateTimeProvider = new();
        [SetUp]
        public void SetUp()
        {
            _context = new DefaultHttpContext();
            _context.Request.Headers[HeaderNames.Authorization] = JWT;

            httpContextAccessor.Setup(_ => _.HttpContext).Returns(_context);
            dateTimeProvider.Setup(_ => _.UtcNow).Returns(DateTime.Parse("23/05/2021"));
            dateTimeProvider.Setup(_ => _.Now).Returns(DateTime.Now);
            _requestContext = new RequestContext(
                httpContextAccessor.Object,
                clientService.Object,
                memberService.Object,
                redisUtil.Object,
                memoryCache.Object,
                mapper.Object,
                premiumService.Object,
                dateTimeProvider.Object);
        }

        private DefaultHttpContext _context;
        private RequestContext _requestContext;

        [Test]
        public void GetCognitoClientId_FromJwt()
        {
            string clientId = _requestContext.CognitoClientId;

            clientId.Should().Be(_clientId);
        }

        [Test]
        public void GetCognitoId_FromJwt()
        {
            string cognitoUserId = _requestContext.CognitoUserId;

            cognitoUserId.Should().Be(_cognitoUserId);
        }

        [Test]
        public void GetCognitoId_ShouldReturnEmptyString_GivenNoAuthorizationHeader()
        {
            httpContextAccessor.Object.HttpContext.Request.Headers.Remove(HeaderNames.Authorization);

            string cognitoUserId = _requestContext.CognitoUserId;

            cognitoUserId.Should().BeEmpty();
        }

        [Test]
        public void GetIsMobile_ShouldReturnFalse_GivenEmptyDeviceHeader()
        {
            httpContextAccessor.Object.HttpContext.Request.Headers.Add("device", "");

            _requestContext.IsMobileDevice.Should().BeFalse();
        }

        [Test]
        public void GetIsMobile_ShouldReturnTrue_GivenMobileDeviceHeader()
        {
            httpContextAccessor.Object.HttpContext.Request.Headers.Add("device", "mobile");

            _requestContext.IsMobileDevice.Should().BeTrue();
        }

        [Test]
        public void GetIsMobile_ShouldReturnTrue_GivenMultipleMobileDeviceHeaders()
        {
            httpContextAccessor.Object.HttpContext.Request.Headers.Add("device", "mobile,mobile");

            _requestContext.IsMobileDevice.Should().BeTrue();
        }

        [Test]
        public void GetIsMobile_ShouldReturnFalse_GivenUserAgentHeaderWithBrowser()
        {
            httpContextAccessor.Object.HttpContext.Request.Headers.Add(HeaderNames.UserAgent, "chrome");

            _requestContext.IsMobileDevice.Should().BeFalse();
        }

        [Test]
        public void GetIsMobile_ShouldReturnTrue_GivenQueryParamaterIsMobileSetToTrue()
        {
            _context.Request.QueryString = new QueryString("?IsMobile=true");

            _requestContext.IsMobileDevice.Should().BeTrue();
        }

        [Test]
        public void GetIsMobile_ShouldReturnTrue_GivenQueryParamaterIsMobileSetToFalse()
        {
            var context = new DefaultHttpContext();

            context.Request.QueryString = new QueryString("?IsMobile=false");

            httpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

            _requestContext.IsMobileDevice.Should().BeFalse();
        }

        [Test]
        public void GetCognitoId_FromExpiredJwt_ShouldThrowNotAuthorizedException()
        {
            dateTimeProvider.Setup(_ => _.UtcNow).Returns(DateTime.UtcNow);
            Exception ex =
            Assert.Throws<NotAuthorizedException>(() => { var cognito = _requestContext.CognitoUserId; });
            Assert.That(ex.Message, Is.EqualTo("Expired Token"));
        }


        
        [TestCase("Notifier/Mozilla", true)]
        [TestCase("notifier/Mozilla", true)]
        [TestCase("Mozilla/Notifier", true)]
        [TestCase("Mozilla", false)]
        [TestCase("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36", false)] 

        public void IsRequestFromNotifier_FromUserAgent(string userAgentValue,bool result)
        {
            _context.Request.Headers[HeaderNames.UserAgent] = userAgentValue;
            httpContextAccessor.Setup(_ => _.HttpContext).Returns(_context);
            _requestContext.IsFromNotifier.Should().Be(result);
          
        }

        [TestCase("Notifier", true)]
        [TestCase("notifier", true)]
        [TestCase("", false)]
        [TestCase("anotherValue", false)]

        public void IsRequestFromNotifier_FromQueryString(string queryStringValue, bool result)
        {
            _context.Request.QueryString = new QueryString($"?utm_source={queryStringValue}");
            httpContextAccessor.Setup(_ => _.HttpContext).Returns(_context);
            _requestContext.IsFromNotifier.Should().Be(result);

        }
    }
}