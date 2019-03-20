using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Application.Discovery;
using Microwave.Eventstores.UnitTests;
using Microwave.WebApi;
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
                    new []{ nameof(Event1), nameof(Event3)},
                    new List<ReadModelSubscription>()),
                discoveryRepo.Object,
                new EventLocation());

            await discoveryHandler.DiscoverConsumingServices();
            var consumingServices = discoveryHandler.GetConsumingServices();

            var consumingService = consumingServices.GetService(typeof(Event1));
            var service = consumingServices.GetService(typeof(Event3));

            Assert.AreEqual("Service1", consumingService.ServiceName);
            Assert.AreEqual("Service3", service.ServiceName);
            Assert.AreEqual(new Uri("http://service1.de/"), consumingService.ServiceBaseAddress);
            Assert.AreEqual(new Uri("http://service3.de" ), service.ServiceBaseAddress);
            Assert.AreEqual("Event1", consumingService.PublishedEventTypes.Single());
            Assert.AreEqual("Event3", service.PublishedEventTypes.Single());
        }
    }
}