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
                PublisherEventConfig(new Uri("http://service1.de"), new[] {"Event1"}, true, "Service1"));

            discoveryRepo.Setup(m => m.GetPublishedEventTypes(new Uri("http://service2.de"))).ReturnsAsync(new
                PublisherEventConfig(new Uri("http://service2.de"), new[] {"Event2"}, true, "Service2"));

            discoveryRepo.Setup(m => m.GetPublishedEventTypes(new Uri("http://Service3.de"))).ReturnsAsync(new
                PublisherEventConfig(new Uri("http://service3.de"), new[] {"Event3", "Event4"}, true, "Service3"));
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

            var consumingService = consumingServices.GetServiceForEvent(typeof(Event1));
            var service = consumingServices.GetServiceForEvent(typeof(Event3));

            Assert.AreEqual("Service1", consumingService.ServiceName);
            Assert.AreEqual("Service3", service.ServiceName);
            Assert.AreEqual(new Uri("http://service1.de/"), consumingService.ServiceBaseAddress);
            Assert.AreEqual(new Uri("http://service3.de" ), service.ServiceBaseAddress);
            Assert.AreEqual("Event1", consumingService.SubscribedEvents.Single());
            Assert.AreEqual("Event3", service.SubscribedEvents.Single());
        }

        [TestMethod]
        public async Task DiscoveryWorkingForReadModels()
        {
            var discoveryRepo = new Mock<IServiceDiscoveryRepository>();
            discoveryRepo.Setup(m => m.GetPublishedEventTypes(new Uri("http://service1.de"))).ReturnsAsync(new
                PublisherEventConfig(new Uri("http://service1.de"), new[] {nameof(TestEv1)}, true, "Service1"));

            discoveryRepo.Setup(m => m.GetPublishedEventTypes(new Uri("http://service2.de"))).ReturnsAsync(new
                PublisherEventConfig(new Uri("http://service2.de"), new[] {nameof(TestEv2)}, true, "Service2"));

            discoveryRepo.Setup(m => m.GetPublishedEventTypes(new Uri("http://Service3.de"))).ReturnsAsync(new
                PublisherEventConfig(new Uri("http://service3.de"), new[] {nameof(TestEv3), "Event4"}, true, "Service3"));
            var discoveryHandler = new DiscoveryHandler(new ServiceBaseAddressCollection
            {
                new Uri("http://service1.de"),
                new Uri("http://service2.de"),
                new Uri("http://service3.de")
            },
                new SubscribedEventCollection(
                    new List<string>(),
                    new List<ReadModelSubscription> {
                        new ReadModelSubscription( nameof(ReadModel1), nameof(TestEv1) ),
                        new ReadModelSubscription( nameof(ReadModel3), nameof(TestEv3) )
                        }
                    ),
                discoveryRepo.Object,
                new EventLocation());

            await discoveryHandler.DiscoverConsumingServices();
            var consumingServices = discoveryHandler.GetConsumingServices();

            var consumingService1 = consumingServices.GetServiceForReadModel(typeof(ReadModel1));
            var consumingService2 = consumingServices.GetServiceForReadModel(typeof(ReadModel2));
            var consumingService3 = consumingServices.GetServiceForReadModel(typeof(ReadModel3));

            Assert.AreEqual(null, consumingService2);
            Assert.AreEqual("Service1", consumingService1.ServiceName);
            Assert.AreEqual("Service3", consumingService3.ServiceName);
            Assert.AreEqual(new Uri("http://service1.de/"), consumingService1.ServiceBaseAddress);
            Assert.AreEqual(new Uri("http://service3.de" ), consumingService3.ServiceBaseAddress);
        }
    }

    class ReadModel1
    {
    }

    class ReadModel2
    {
    }

    class ReadModel3
    {
    }
}