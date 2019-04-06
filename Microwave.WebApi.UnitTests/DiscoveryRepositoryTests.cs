using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Application.Discovery;
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
            var serviceDependenciesDto = new ServiceDependenciesDto
            {
                ServiceName = "TestService",
                DependantServices = new List<ServiceDependenciesDto>
                {
                    new ServiceDependenciesDto
                    {
                        ServiceName = "RemoteName1",
                        ServiceBaseAddress = new Uri("http://remoteservice.de")
                    },
                    new ServiceDependenciesDto
                    {
                        ServiceName = "RemoteName2",
                        ServiceBaseAddress = new Uri("http://remoteservice2.de")
                    }
                }
            };
            var mockHttp = new MockHttpMessageHandler();
            var serializeObject = JsonConvert.SerializeObject(serviceDependenciesDto);
            mockHttp.When("http://localhost:5000/Dicovery/ServiceDependencies")
                .Respond("application/json", serializeObject);

            var client = new DiscoveryClient(mockHttp);
            client.BaseAddress = new Uri("http://localhost:5000/");

            var mock = new Mock<IDiscoveryClientFactory>();
            mock.Setup(m => m.GetClient(It.IsAny<Uri>())).Returns(client);
            var serviceDiscoveryRepository = new ServiceDiscoveryRepository(mock.Object);
            var serviceAdress = new Uri("http://localhost:5000/");

            var dependantServices = await serviceDiscoveryRepository.GetDependantServices(serviceAdress);

            Assert.AreEqual(2, dependantServices.DependantServices.Count());
        }
    }
}