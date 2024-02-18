using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Cashrewards3API.Tests.Helpers
{
    public class Request
    {
        public Request(string requestUri, HttpContent content)
        {
            RequestUri = requestUri;
            Body = content?.ReadAsStringAsync().Result;
        }

        public Request(string requestUri, string body = "")
        {
            RequestUri = requestUri;
            Body = body;
        }

        public string RequestUri { get; set; }
        public string Body { get; set; }
    }

    public class HttpClientFactoryMock : Mock<IHttpClientFactory>
    {
        public Dictionary<string, HttpClient> Clients { get; } = new();

        public Dictionary<string, Mock<HttpMessageHandler>> MessageHandlers { get; } = new();

        public List<Request> Requests { get; } = new();

        public HttpClient CreateClientMock(string name, Uri baseAddress)
        {
            MessageHandlers[name] = new Mock<HttpMessageHandler>();
            Clients[name] = new HttpClient(MessageHandlers[name].Object);
            Clients[name].BaseAddress = baseAddress;
            Setup(x => x.CreateClient(name)).Returns(Clients[name]);
            return Clients[name];
        }

        private void SetupClient(string name, Func<HttpResponseMessage> response)
        {
            MessageHandlers[name]
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Callback((HttpRequestMessage request, CancellationToken cancellationToken) => Requests.Add(new Request(request.RequestUri.ToString(), request.Content)))
                .ReturnsAsync(response.Invoke());
        }

        public void SetupClientSendAsyncWithResponse(string name, string responseString)
        {
            SetupClient(name, () => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseString)
            });
        }

        public void SetupClientSendAsyncWithJsonResponse<T>(string name, T response)
        {
            SetupClient(name, () => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(response)
            });
        }

        public void SetupClientGetRequests(string name, HttpStatusCode statusCode)
        {
            SetupClient(name, () => new HttpResponseMessage
            {
                StatusCode = statusCode
            });
        }
    }
}
