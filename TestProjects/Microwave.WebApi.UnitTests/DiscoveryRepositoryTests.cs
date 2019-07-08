using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Discovery.EventLocations;
using Microwave.Discovery.ServiceMaps;
using Microwave.WebApi.Discovery;
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
            var serviceDiscoveryRepository = new DiscoveryRepository(mock.Object);
            var serviceAdress = new Uri("http://localhost:5000/");
            var publishedEventTypes = serviceDiscoveryRepository.GetPublishedEventTypes(serviceAdress);

            var publisherEventConfig = publishedEventTypes.Result;
            Assert.AreEqual(serviceAdress, publisherEventConfig.ServiceEndPoint.ServiceBaseAddress);
            var eventTypes = publisherEventConfig.PublishedEventTypes.ToList();
            Assert.AreEqual(2, eventTypes.Count);
            Assert.AreEqual("event1", eventTypes[0].Name);
            Assert.AreEqual("event2", eventTypes[1].Name);
            Assert.AreEqual("MeinService", publisherEventConfig.ServiceEndPoint.Name);
        }

        [TestMethod]
        public async Task GetServiceDependencies()
        {
            var serviceNodeWithDependentServicesDto = new ServiceNodeWithDependantServices(
                "ServiceName",
                new List<ServiceEndPoint>
                {
                    new ServiceEndPoint(new Uri("http://remoteservice1.de"), "RemoteName1"),
                    new ServiceEndPoint(new Uri("http://remoteservice2.de"), "RemoteName2"),
                });

            var mockHttp = new MockHttpMessageHandler();
            var serializeObject = JsonConvert.SerializeObject(serviceNodeWithDependentServicesDto);
            mockHttp.When("http://localhost:5000/Dicovery/ServiceDependencies")
                .Respond("application/json", serializeObject);

            var client = new DiscoveryClient(mockHttp);
            client.BaseAddress = new Uri("http://localhost:5000/");

            var mock = new Mock<IDiscoveryClientFactory>();
            mock.Setup(m => m.GetClient(It.IsAny<Uri>())).Returns(client);
            var serviceDiscoveryRepository = new DiscoveryRepository(mock.Object);
            var serviceAdress = new Uri("http://localhost:5000/");

            var serviceNode = await serviceDiscoveryRepository.GetDependantServices(serviceAdress);
            var dependantServices = serviceNode.Services.ToList();
            Assert.AreEqual(2, dependantServices.Count);
            Assert.AreEqual("ServiceName", serviceNode.ServiceEndPoint.Name);
            Assert.AreEqual(new Uri("http://localhost:5000/"), serviceNode.ServiceEndPoint.ServiceBaseAddress);
            Assert.AreEqual("RemoteName1", dependantServices[0].Name);
            Assert.AreEqual("RemoteName2", dependantServices[1].Name);
            Assert.AreEqual(new Uri("http://remoteservice1.de"), dependantServices[0].ServiceBaseAddress);
            Assert.AreEqual(new Uri("http://remoteservice2.de"), dependantServices[1].ServiceBaseAddress);
        }
    }
}