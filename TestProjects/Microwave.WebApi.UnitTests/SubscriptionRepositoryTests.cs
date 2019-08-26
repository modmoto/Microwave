using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Discovery.Subscriptions;
using Microwave.WebApi.Discovery;
using Moq;
using RichardSzalay.MockHttp;

namespace Microwave.WebApi.UnitTests
{
    [TestClass]
    public class SubscriptionRepositoryTests
    {
        [TestMethod]
        public async Task GetClientDefault()
        {
            var microwaveHttpContext = new MicrowaveHttpContext();
            microwaveHttpContext.Configure(new Uri("http://123.de"));

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, "http://456.de/Discovery/Subscriptions")
                .Respond(HttpStatusCode.OK);

            var client = new HttpClient(mockHttp);
            client.BaseAddress = new Uri("http://123.de");

            var mock = new Mock<IMicrowaveHttpClientFactory>();
            mock.Setup(m => m.CreateHttpClient(It.IsAny<Uri>())).ReturnsAsync(client);

            var subscriptionRepository = new SubscriptionRepository(mock.Object, microwaveHttpContext);
            await subscriptionRepository.SubscribeForEvent(new Subscription("ev", new Uri("http://456.de")));
        }
    }
}