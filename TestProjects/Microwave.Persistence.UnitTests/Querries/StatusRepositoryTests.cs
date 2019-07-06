using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Discovery.EventLocations;
using Microwave.Discovery.ServiceMaps;
using Microwave.Persistence.UnitTestSetupPorts;

namespace Microwave.Persistence.UnitTests.Querries
{
    [TestClass]
    public class StatusRepositoryTests
    {
        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task StatusLoadAndSafe_HappyPath(IPersistenceDefinition definition)
        {
            var statusRepository = definition.StatusRepository;

            List<EventsPublishedByService> services = new List<EventsPublishedByService> {
                EventsPublishedByService.Reachable(new ServiceEndPoint(new Uri("http://service1.de"), "Name1"), new []
                {
                    new EventSchema("Event1"),
                    new EventSchema("Event2"),
                    new EventSchema("Event3")
                })
            };
            var subscribedEventCollection = new EventsSubscribedByService(
                new []
                {
                    new EventSchema("Event1"),
                    new EventSchema("Event2")
                },
                new []{ new ReadModelSubscription("Rm1", new EventSchema("Event3")) });
            var eventLocation = new EventLocation(services, subscribedEventCollection);

            await statusRepository.SaveEventLocation(eventLocation);
            var location = await statusRepository.GetEventLocation();

            Assert.IsTrue(!location.UnresolvedEventSubscriptions.Any());
            Assert.IsTrue(!location.UnresolvedReadModeSubscriptions.Any());
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task StatusLoadAndSafe_NoSaveBeforeGet(IPersistenceDefinition definition)
        {
            var statusRepository = definition.StatusRepository;

            var location = await statusRepository.GetEventLocation();

            Assert.IsNotNull(location);
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task StatusLoadAndSafe_OneUnresolvedEvent(IPersistenceDefinition definition)
        {
            var statusRepository = definition.StatusRepository;

            List<EventsPublishedByService> services = new List<EventsPublishedByService> {
                EventsPublishedByService.Reachable(new ServiceEndPoint(new Uri("http://service1.de"), "Name1"), new []
                {
                    new EventSchema("Event1"),
                    new EventSchema("Event3")
                })
            };
            var subscribedEventCollection = new EventsSubscribedByService(
                new []
                {
                    new EventSchema("Event1"),
                    new EventSchema("Event2")
                },
                new []{ new ReadModelSubscription("Rm1", new EventSchema("Event3")) });
            var eventLocation = new EventLocation(services, subscribedEventCollection);

            await statusRepository.SaveEventLocation(eventLocation);
            var location = await statusRepository.GetEventLocation();

            Assert.AreEqual("Event2", location.UnresolvedEventSubscriptions.Single().Name);
            Assert.IsTrue(!location.UnresolvedReadModeSubscriptions.Any());
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task StatusLoadAndSafe_OneUnresolvedReadModel(IPersistenceDefinition definition)
        {
            var statusRepository = definition.StatusRepository;

            List<EventsPublishedByService> services = new List<EventsPublishedByService> {
                EventsPublishedByService.Reachable(new ServiceEndPoint(new Uri("http://service1.de"), "Name1"), new []
                {
                    new EventSchema("Event1"),
                    new EventSchema("Event2")
                })
            };
            var subscribedEventCollection = new EventsSubscribedByService(
                new []
                {
                    new EventSchema("Event1"),
                    new EventSchema("Event2")
                },
                new []{ new ReadModelSubscription("Rm1", new EventSchema("Event3")) });
            var eventLocation = new EventLocation(services, subscribedEventCollection);

            await statusRepository.SaveEventLocation(eventLocation);
            await statusRepository.SaveEventLocation(eventLocation);
            var location = await statusRepository.GetEventLocation();

            Assert.AreEqual("Rm1", location.UnresolvedReadModeSubscriptions.Single().ReadModelName);
            Assert.AreEqual("Event3", location.UnresolvedReadModeSubscriptions.Single().GetsCreatedOn.Name);
            Assert.IsTrue(!location.UnresolvedEventSubscriptions.Any());
        }


        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task SaveAndLoadServiceMap(IPersistenceDefinition definition)
        {
            var statusRepository = definition.StatusRepository;

            var map = new ServiceMap(new List<ServiceNodeConfig>
            {
                new ServiceNodeConfig(
                    new ServiceEndPoint(new Uri("http://123.de"), "Name"),
                    new List<ServiceEndPoint>
                    {
                        new ServiceEndPoint(new Uri("http://www.uri1.de"), "Name")
                    },
                    true),
                new ServiceNodeConfig(
                    new ServiceEndPoint(new Uri("http://123.de"), "Name2"),
                    new List<ServiceEndPoint>
                    {
                        new ServiceEndPoint(new Uri("http://www.uri2.de"), "Name2"),
                        new ServiceEndPoint(new Uri("http://www.uri1.de"), "Name")
                    },
                    true),
                new ServiceNodeConfig(
                    new ServiceEndPoint(new Uri("http://123.de")),
                    new List<ServiceEndPoint>(),
                    false)
            });
            await statusRepository.SaveServiceMap(map);
            var mapLoaded = await statusRepository.GetServiceMap();

            var serviceDependenciesDtos = mapLoaded.AllServices.ToList();
            Assert.AreEqual("Name", serviceDependenciesDtos[0].ServiceEndPoint.Name);
            Assert.AreEqual("Name2", serviceDependenciesDtos[1].ServiceEndPoint.Name);
            Assert.IsTrue(serviceDependenciesDtos[1].IsReachable);
            Assert.IsFalse(serviceDependenciesDtos[2].IsReachable);
            Assert.AreEqual(new Uri("http://www.uri1.de"), serviceDependenciesDtos[0].Services.First().ServiceBaseAddress);
            Assert.AreEqual(new Uri("http://www.uri2.de"), serviceDependenciesDtos[1].Services.First().ServiceBaseAddress);
            Assert.AreEqual(new Uri("http://www.uri1.de"), serviceDependenciesDtos[1].Services.Skip(1).First().ServiceBaseAddress);
            Assert.AreEqual("Name", serviceDependenciesDtos[0].Services.First().Name);
            Assert.AreEqual("Name2", serviceDependenciesDtos[1].Services.First().Name);
            Assert.AreEqual("Name", serviceDependenciesDtos[1].Services.Skip(1).First().Name);
        }
    }
}