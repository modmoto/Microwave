using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Application.Discovery;
using Microwave.Eventstores.UnitTests;
using Moq;

namespace Microwave.Application.UnitTests
{
    [TestClass]
    public class DiscoveryHandlerTests : IntegrationTests
    {
        [TestMethod]
        public async Task DiscoveryWorkingForIHandleAsyncEvents()
        {
            var discoveryRepo = new Mock<IServiceDiscoveryRepository>();
            discoveryRepo.Setup(m => m.GetPublishedEventTypes(new Uri("http://service1.de"))).ReturnsAsync(new
                ConsumingService(new Uri("http://service1.de"), new[] {"Event1"}, "Service1"));

            discoveryRepo.Setup(m => m.GetPublishedEventTypes(new Uri("http://service2.de"))).ReturnsAsync(new
                ConsumingService(new Uri("http://service2.de"), new[] {"Event2"}, "Service2"));

            discoveryRepo.Setup(m => m.GetPublishedEventTypes(new Uri("http://Service3.de"))).ReturnsAsync(new
                ConsumingService(new Uri("http://service3.de"), new[] {"Event3", "Event4"}, "Service3"));
            var discoveryHandler = new DiscoveryHandler(new ServiceBaseAddressCollection
            {
                new Uri("http://service1.de"),
                new Uri("http://service2.de"),
                new Uri("http://service3.de")
            },
                new SubscribedEventCollection(
                    new []{ "Event1", "Event3"},
                    new List<ReadModelSubscription>()),
                discoveryRepo.Object);

            var consumingServices = await discoveryHandler.GetConsumingServices();

            Assert.AreEqual(2, consumingServices.Count);
            Assert.AreEqual("Service1", consumingServices[0].ServiceName);
            Assert.AreEqual("Service3", consumingServices[1].ServiceName);
            Assert.AreEqual(new Uri("http://service1.de/"), consumingServices[0].ServiceBaseAddress);
            Assert.AreEqual(new Uri("http://service3.de" ), consumingServices[1].ServiceBaseAddress);
            Assert.AreEqual("Event1", consumingServices[0].PublishedEventTypes.Single());
            Assert.AreEqual("Event3", consumingServices[1].PublishedEventTypes.Single());
        }
    }
}