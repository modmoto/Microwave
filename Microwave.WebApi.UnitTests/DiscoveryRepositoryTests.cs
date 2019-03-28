using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
                .Respond("application/json", "[ \"event1\", \"event2]");

            var client = new DiscoveryClient(mockHttp);

            var serviceDiscoveryRepository = new ServiceDiscoveryRepository(client);
            var serviceAdress = new Uri("http://localost:5000/");
            var publishedEventTypes = serviceDiscoveryRepository.GetPublishedEventTypes(serviceAdress);

            var publisherEventConfig = publishedEventTypes.Result;
            Assert.AreEqual(serviceAdress, publisherEventConfig.ServiceBaseAddress);
            Assert.AreEqual(new List<string> { "event1", "event2"}, publisherEventConfig.PublishedEventTypes);
        }
    }
}