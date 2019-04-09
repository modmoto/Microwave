using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Discovery;
using Moq;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;

namespace Microwave.WebApi.UnitTests
{
    [TestClass]
    public class DiscoveryRepositoryTests
    {
        [TestMethod]
        public void GetPublishedEvents()
        {
            var publishedEvents = new PublishedEventsByServiceDto
            {
                ServiceName = "MeinService",
                PublishedEvents = {new EventSchema("event1"), new EventSchema("event2")}
            };
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("http://localhost:5000/Dicovery/PublishedEvents")
                .Respond("application/json", JsonConvert.SerializeObject(publishedEvents));

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
            Assert.AreEqual("event1", eventTypes[0].Name);
            Assert.AreEqual("event2", eventTypes[1].Name);
            Assert.AreEqual("MeinService", publisherEventConfig.ServiceName);
        }

        [TestMethod]
        public async Task GetServiceDependencies()
        {
            var services = new List<MicrowaveService>()
            {
                new MicrowaveService(
                    new Uri("http://remoteservice1.de"),
                    new List<EventSchema>(),
                    new List<ReadModelSubscription>(),
                    "RemoteName1"),
                new MicrowaveService(
                    new Uri("http://remoteservice2.de"),
                    new List<EventSchema>(),
                    new List<ReadModelSubscription>(),
                    "RemoteName2")
            };

            var eventLocationMock = new EventLocationMock(services);

            var mockHttp = new MockHttpMessageHandler();
            var serializeObject = JsonConvert.SerializeObject(eventLocationMock);
            mockHttp.When("http://localhost:5000/Dicovery/ServiceDependencies")
                .Respond("application/json", serializeObject);

            var client = new DiscoveryClient(mockHttp);
            client.BaseAddress = new Uri("http://localhost:5000/");

            var mock = new Mock<IDiscoveryClientFactory>();
            mock.Setup(m => m.GetClient(It.IsAny<Uri>())).Returns(client);
            var serviceDiscoveryRepository = new ServiceDiscoveryRepository(mock.Object);
            var serviceAdress = new Uri("http://localhost:5000/");

            var serviceNode = await serviceDiscoveryRepository.GetDependantServices(serviceAdress);
            var dependantServices = serviceNode.Services.ToList();
            Assert.AreEqual(2, dependantServices.Count);
            Assert.AreEqual(new Uri("http://localhost:5000/"), serviceNode.ServiceBaseAddress);
            Assert.AreEqual("RemoteName1", dependantServices[0].ServiceName);
            Assert.AreEqual("RemoteName2", dependantServices[1].ServiceName);
            Assert.AreEqual(new Uri("http://remoteservice1.de"), dependantServices[0].ServiceBaseAddress);
            Assert.AreEqual(new Uri("http://remoteservice2.de"), dependantServices[1].ServiceBaseAddress);
        }
    }

    public class EventLocationMock : IEventLocation
    {
        public EventLocationMock(IEnumerable<MicrowaveService> services)
        {
            Services = services;
        }

        public MicrowaveService GetServiceForEvent(Type eventType)
        {
            return null;
        }

        public MicrowaveService GetServiceForReadModel(Type readModel)
        {
            return null;
        }

        public IEnumerable<MicrowaveService> Services { get; }
        public IEnumerable<EventSchema> UnresolvedEventSubscriptions => new List<EventSchema>();
        public IEnumerable<ReadModelSubscription> UnresolvedReadModeSubscriptions => new List<ReadModelSubscription>();
    }
}