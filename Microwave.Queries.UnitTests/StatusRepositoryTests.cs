using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Application;
using Microwave.Application.Discovery;
using Microwave.Application.Exceptions;
using Microwave.Application.Results;
using Microwave.Domain;
using Microwave.Eventstores.UnitTests;

namespace Microwave.Queries.UnitTests
{
    [TestClass]
    public class StatusRepositoryTests : IntegrationTests
    {
        [TestMethod]
        public async Task StatusLoadAndSafe_HappyPath()
        {
            var statusRepository = new StatusRepository(ReadModelDatabase);

            List<PublisherEventConfig> services = new List<PublisherEventConfig> {
                new PublisherEventConfig(new Uri("http://service1.de"), new []{ "Event1", "Event2", "Event3"})
            };
            var subscribedEventCollection = new SubscribedEventCollection(
                new []{ "Event1", "Event2" },
                new []{ new ReadModelSubscription("Rm1", "Event3") });
            var eventLocation = new EventLocation(services, subscribedEventCollection);

            await statusRepository.SaveEventLocation(eventLocation);
            var location = await statusRepository.GetEventLocation();

            Assert.IsTrue(!location.UnresolvedEventSubscriptions.Any());
            Assert.IsTrue(!location.UnresolvedReadModeSubscriptions.Any());
        }

        [TestMethod]
        public async Task StatusLoadAndSafe_OneUnresolvedEvent()
        {
            var statusRepository = new StatusRepository(ReadModelDatabase);

            List<PublisherEventConfig> services = new List<PublisherEventConfig> {
                new PublisherEventConfig(new Uri("http://service1.de"), new []{ "Event1", "Event3"})
            };
            var subscribedEventCollection = new SubscribedEventCollection(
                new []{ "Event1", "Event2" },
                new []{ new ReadModelSubscription("Rm1", "Event3") });
            var eventLocation = new EventLocation(services, subscribedEventCollection);

            await statusRepository.SaveEventLocation(eventLocation);
            var location = await statusRepository.GetEventLocation();

            Assert.AreEqual("Event2", location.UnresolvedEventSubscriptions.Single());
            Assert.IsTrue(!location.UnresolvedReadModeSubscriptions.Any());
        }

        [TestMethod]
        public async Task StatusLoadAndSafe_OneUnresolvedReadModel()
        {
            var statusRepository = new StatusRepository(ReadModelDatabase);

            List<PublisherEventConfig> services = new List<PublisherEventConfig> {
                new PublisherEventConfig(new Uri("http://service1.de"), new []{ "Event1", "Event2"})
            };
            var subscribedEventCollection = new SubscribedEventCollection(
                new []{ "Event1", "Event2" },
                new []{ new ReadModelSubscription("Rm1", "Event3") });
            var eventLocation = new EventLocation(services, subscribedEventCollection);

            await statusRepository.SaveEventLocation(eventLocation);
            var location = await statusRepository.GetEventLocation();

            Assert.AreEqual("Rm1", location.UnresolvedReadModeSubscriptions.Single().ReadModelName);
            Assert.AreEqual("Event3", location.UnresolvedReadModeSubscriptions.Single().GetsCreatedOn);
            Assert.IsTrue(!location.UnresolvedEventSubscriptions.Any());
        }
    }
}