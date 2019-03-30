using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RichardSzalay.MockHttp;

namespace Microwave.WebApi.UnitTests
{
    [TestClass]
    public class DiscoveryRepositoryTests
    {
        [TestMethod]
        public async Task GetPublishedEvents()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("http://localost:5000/Dicovery/PublishedEvents")
                .Respond("application/json", "[ \"event1\", \"event2\"]");

            var client = new DiscoveryClient(mockHttp);

            var mock = new Mock<IDiscoveryClientFactory>();
            mock.Setup(m => m.GetClient(It.IsAny<Uri>())).Returns(client);
            var serviceDiscoveryRepository = new ServiceDiscoveryRepository(mock.Object);
            var serviceAdress = new Uri("http://localost:5000/");
            var publishedEventTypes = serviceDiscoveryRepository.GetPublishedEventTypes(serviceAdress);

            var publisherEventConfig = publishedEventTypes.Result;
            Assert.AreEqual(serviceAdress, publisherEventConfig.ServiceBaseAddress);
            var eventTypes = publisherEventConfig.PublishedEventTypes.ToList();
            Assert.AreEqual(2, eventTypes.Count);
            Assert.AreEqual("event1", eventTypes[0]);
            Assert.AreEqual("event2", eventTypes[1]);
        }
    }
}