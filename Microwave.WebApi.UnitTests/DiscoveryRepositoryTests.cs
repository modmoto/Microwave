using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RichardSzalay.MockHttp;

namespace Microwave.WebApi.UnitTests
{
    [TestClass]
    public class DiscoveryRepositoryTests
    {
        [TestMethod]
        public void GetPublishedEvents()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("http://localhost:5000/Dicovery/PublishedEvents")
                .Respond("application/json", "{ \"ServiceName\": \"MeinService\", \"PublishedEvents\":[ \"event1\", \"event2\"]}");

            var client = new DiscoveryClient(mockHttp);
            client.BaseAddress = new Uri("http://localhost:5000/");

            var mock = new Mock<IDiscoveryClientFactory>();
            mock.Setup(m => m.GetClient(It.IsAny<Uri>())).Returns(client);
            var serviceDiscoveryRepository = new ServiceDiscoveryRepository(mock.Object);
            var serviceAdress = new Uri("http://localhost:5000/");
            var publishedEventTypes = serviceDiscoveryRepository.GetPublishedEventTypes(serviceAdress);

            var publisherEventConfig = publishedEventTypes.Result;
            Assert.AreEqual(serviceAdress, publisherEventConfig.ServiceBaseAddress);
            var eventTypes = publisherEventConfig.PublishedEventTypes.ToList();
            Assert.AreEqual(2, eventTypes.Count);
            Assert.AreEqual("event1", eventTypes[0]);
            Assert.AreEqual("event2", eventTypes[1]);
            Assert.AreEqual("MeinService", publisherEventConfig.ServiceName);
        }
    }
}