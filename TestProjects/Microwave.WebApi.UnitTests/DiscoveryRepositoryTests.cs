using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Discovery.EventLocations;
using Microwave.Discovery.ServiceMaps;
using Microwave.Logging;
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

            var client = new HttpClient(mockHttp);
            client.BaseAddress = new Uri("http://localhost:5000/");

            var mock = new Mock<IDiscoveryClientFactory>();
            mock.Setup(m => m.GetClient(It.IsAny<Uri>())).ReturnsAsync(client);
            var serviceDiscoveryRepository = new DiscoveryRepository(mock.Object, new MicrowaveLogger<DiscoveryRepository>());
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
            var serviceNodeWithDependentServicesDto = MicrowaveServiceNode.ReachableMicrowaveServiceNode(new ServiceEndPoint(null, "ServiceName"),
                new List<ServiceEndPoint>
                {
                    new ServiceEndPoint(new Uri("http://remoteservice1.de"), "RemoteName1"),
                    new ServiceEndPoint(new Uri("http://remoteservice2.de"), "RemoteName2"),
                });

            var mockHttp = new MockHttpMessageHandler();
            var serializeObject = JsonConvert.SerializeObject(serviceNodeWithDependentServicesDto);
            mockHttp.When("http://localhost:5000/Dicovery/ServiceDependencies")
                .Respond("application/json", serializeObject);

            var client = new HttpClient(mockHttp);
            client.BaseAddress = new Uri("http://localhost:5000/");

            var mock = new Mock<IDiscoveryClientFactory>();
            mock.Setup(m => m.GetClient(It.IsAny<Uri>())).ReturnsAsync(client);
            var serviceDiscoveryRepository = new DiscoveryRepository(mock.Object, new MicrowaveLogger<DiscoveryRepository>());
            var serviceAddress = new Uri("http://localhost:5000/");

            var serviceNode = await serviceDiscoveryRepository.GetDependantServices(serviceAddress);
            var dependantServices = serviceNode.ConnectedServices.ToList();
            Assert.AreEqual(2, dependantServices.Count);
            Assert.AreEqual("ServiceName", serviceNode.ServiceEndPoint.Name);
            Assert.AreEqual(new Uri("http://localhost:5000/"), serviceNode.ServiceEndPoint.ServiceBaseAddress);
            Assert.AreEqual("RemoteName1", dependantServices[0].Name);
            Assert.AreEqual("RemoteName2", dependantServices[1].Name);
            Assert.AreEqual(new Uri("http://remoteservice1.de"), dependantServices[0].ServiceBaseAddress);
            Assert.AreEqual(new Uri("http://remoteservice2.de"), dependantServices[1].ServiceBaseAddress);
        }

        [TestMethod]
        public async Task GetServiceDependencies_Unauthorized()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("http://localhost:5000/Dicovery/ServiceDependencies")
                .Respond(HttpStatusCode.Unauthorized);

            var client = new HttpClient(mockHttp);
            client.BaseAddress = new Uri("http://localhost:5000/");

            var mock = new Mock<IDiscoveryClientFactory>();
            mock.Setup(m => m.GetClient(It.IsAny<Uri>())).ReturnsAsync(client);
            var serviceDiscoveryRepository = new DiscoveryRepository(mock.Object, new MicrowaveLogger<DiscoveryRepository>());
            var serviceAddress = new Uri("http://localhost:5000/");

            var serviceNode = await serviceDiscoveryRepository.GetDependantServices(serviceAddress);

            Assert.IsFalse(serviceNode.IsReachable);
        }

        [TestMethod]
        public async Task GetClientDefault()
        {
            var discoveryClientFactory = new DiscoveryClientFactory(new DefaultMicrowaveHttpClientFactory());
            var client = await discoveryClientFactory.GetClient(new Uri("http://123.de"));

            Assert.AreEqual(client.BaseAddress, new Uri("http://123.de"));
        }
    }
}