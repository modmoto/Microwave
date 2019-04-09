using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Discovery;
using Microwave.Persistence.MongoDb.Querries;
using Microwave.Persistence.MongoDb.UnitTests.Eventstores;

namespace Microwave.Persistence.MongoDb.UnitTests.Querries
{
    [TestClass]
    public class StatusRepositoryTests : IntegrationTests
    {
        [TestMethod]
        public async Task StatusLoadAndSafe_HappyPath()
        {
            var statusRepository = new StatusRepository(EventDatabase);

            List<EventsPublishedByService> services = new List<EventsPublishedByService> {
                new EventsPublishedByService(new Uri("http://service1.de"), new []
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

        [TestMethod]
        public async Task StatusLoadAndSafe_NoSaveBeforeGet()
        {
            var statusRepository = new StatusRepository(EventDatabase);

            var location = await statusRepository.GetEventLocation();

            Assert.IsNull(location);
        }

        [TestMethod]
        public async Task StatusLoadAndSafe_OneUnresolvedEvent()
        {
            var statusRepository = new StatusRepository(EventDatabase);

            List<EventsPublishedByService> services = new List<EventsPublishedByService> {
                new EventsPublishedByService(new Uri("http://service1.de"), new []
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

        [TestMethod]
        public async Task StatusLoadAndSafe_OneUnresolvedReadModel()
        {
            var statusRepository = new StatusRepository(EventDatabase);

            List<EventsPublishedByService> services = new List<EventsPublishedByService> {
                new EventsPublishedByService(new Uri("http://service1.de"), new []
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

        [TestMethod]
        public async Task LoadServiceMap()
        {
            var statusRepository = new StatusRepository(EventDatabase);

            var services = new List<EventsPublishedByService> {
                new EventsPublishedByService(new Uri("http://service1.de"), new []
                {
                    new EventSchema("Event1"),
                }, true, "Name1"),
                new EventsPublishedByService(new Uri("http://service2.de"), new []
                {
                    new EventSchema("Event2")
                }, true, "Name2")
            };
            var subscribedEventCollection = new EventsSubscribedByService(
                new []
                {
                    new EventSchema("Event1"),
                    new EventSchema("Event2")
                },
                new List<ReadModelSubscription>());

            var eventLocation = new EventLocation(services, subscribedEventCollection);

            await statusRepository.SaveEventLocation(eventLocation);
            var map = await statusRepository.GetServiceMap();

            var allServices = map.AllServices.ToList();
            Assert.AreEqual("Name1", allServices[0].ServiceName);
            Assert.AreEqual("Name2", allServices[1].ServiceName);
            Assert.AreEqual(new Uri("http://service1.de"), allServices[0].ServiceBaseAddress);
            Assert.AreEqual(new Uri("http://service2.de"), allServices[1].ServiceBaseAddress);
        }
    }
}