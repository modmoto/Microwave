using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Discovery.EventLocations;
using Microwave.Discovery.ServiceMaps;
using Microwave.Discovery.Subscriptions;
using Microwave.Persistence.MongoDb.Querries;
using Microwave.Persistence.UnitTestsSetup.MongoDb;
using Moq;

namespace Microwave.Discovery.UnitTests
{
    [TestClass]
    public class SubscriptionHandlerTests : IntegrationTests
    {
        [TestMethod]
        public async Task SubscribeOnServices()
        {
            var mock = new Mock<IRemoteSubscriptionRepository>();

            var statusRepository = new StatusRepositoryMongoDb(EventMongoDb, new EventLocationCache());
            var eventSchemata = new List<EventSchema> {new EventSchema("egal"), new EventSchema("egal2" )};
            await statusRepository.SaveEventLocation(
                new EventLocation(
                    new List<EventsPublishedByService> { EventsPublishedByService.Reachable(
                            new ServiceEndPoint(new Uri("http://123.de")), eventSchemata)},
                    new EventsSubscribedByService(
                        eventSchemata,
                        new List<ReadModelSubscription>())
                ));

            var discoveryHandler = new SubscriptionHandler(
                new EventsSubscribedByService(eventSchemata, new List<ReadModelSubscription>()),
                statusRepository,
                mock.Object,
                null);

            await discoveryHandler.SubscribeOnDiscoveredServices();

            mock.Verify(m => m.SubscribeForEvent(It.IsAny<Subscription>()), Times.Exactly(2));
        }
    }
}