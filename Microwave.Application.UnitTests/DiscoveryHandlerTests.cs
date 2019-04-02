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
                PublisherEventConfig(new Uri("http://service3.de"), new[] {"Event3", "Event2"}, true, "Service3"));
            var subscribedEventCollection = new SubscribedEventCollection(
                new []{ nameof(Event1), nameof(Event3), nameof(Event4)},
                new List<ReadModelSubscription>());
            var discoveryHandler = new DiscoveryHandler(new ServiceBaseAddressCollection
            {
                new Uri("http://service1.de"),
                new Uri("http://service2.de"),
                new Uri("http://service3.de")
            },
                subscribedEventCollection,
                discoveryRepo.Object,
                new Mock<IStatusRepository>().Object);

            await discoveryHandler.DiscoverConsumingServices();
            var consumingServices = await discoveryHandler.GetConsumingServices();

            var consumingService = consumingServices.GetServiceForEvent(typeof(Event1));
            var service = consumingServices.GetServiceForEvent(typeof(Event3));

            Assert.AreEqual("Service1", consumingService.ServiceName);
            Assert.AreEqual("Service3", service.ServiceName);
            Assert.AreEqual(new Uri("http://service1.de/"), consumingService.ServiceBaseAddress);
            Assert.AreEqual(new Uri("http://service3.de" ), service.ServiceBaseAddress);
            Assert.AreEqual("Event1", consumingService.SubscribedEvents.Single());
            Assert.AreEqual("Event3", service.SubscribedEvents.Single());
            Assert.AreEqual("Event4", consumingServices.UnresolvedEventSubscriptions.Single());
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
                PublisherEventConfig(new Uri("http://service3.de"), new[] {nameof(TestEv3), "Event2"}, true, "Service3"));
            var subscribedEventCollection = new SubscribedEventCollection(
                new List<string>(),
                new List<ReadModelSubscription> {
                    new ReadModelSubscription( nameof(ReadModel1), nameof(TestEv1) ),
                    new ReadModelSubscription( nameof(ReadModel3), nameof(TestEv3) ),
                    new ReadModelSubscription( nameof(ReadModel4), nameof(Event4) ),
                }
            );
            var discoveryHandler = new DiscoveryHandler(new ServiceBaseAddressCollection
            {
                new Uri("http://service1.de"),
                new Uri("http://service2.de"),
                new Uri("http://service3.de")
            },
                subscribedEventCollection,
                discoveryRepo.Object,
                new Mock<IStatusRepository>().Object);

            await discoveryHandler.DiscoverConsumingServices();
            var consumingServices = await discoveryHandler.GetConsumingServices();

            var consumingService1 = consumingServices.GetServiceForReadModel(typeof(ReadModel1));
            var consumingService2 = consumingServices.GetServiceForReadModel(typeof(ReadModel2));
            var consumingService3 = consumingServices.GetServiceForReadModel(typeof(ReadModel3));

            Assert.AreEqual(null, consumingService2);
            Assert.AreEqual("Service1", consumingService1.ServiceName);
            Assert.AreEqual("Service3", consumingService3.ServiceName);
            Assert.AreEqual(new Uri("http://service1.de/"), consumingService1.ServiceBaseAddress);
            Assert.AreEqual(new Uri("http://service3.de" ), consumingService3.ServiceBaseAddress);
            Assert.AreEqual("ReadModel4", consumingServices.UnresolvedReadModeSubscriptions.Single().ReadModelName);
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

    class ReadModel4
    {
    }
}