using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Discovery.EventLocations;
using Microwave.Persistence.MongoDb.Querries;
using Microwave.Persistence.MongoDb.UnitTestsSetup;
using Microwave.WebApi.Discovery;

namespace Microwave.Discovery.UnitTests
{
    [TestClass]
    public class DiscoveryHandlerTests : IntegrationTests
    {
        [TestMethod]
        public async Task GetConsumingServices()
        {
            var statusRepository = new StatusRepository(EventMongoDb, new EventLocationCache());
            await statusRepository.SaveEventLocation(new EventLocation(new List<EventsPublishedByService>(), new EventsSubscribedByService(new List<EventSchema>(), new List<ReadModelSubscription>())
        ));

        var discoveryHandler = new DiscoveryHandler(
                new ServiceBaseAddressCollection(),
                new EventsSubscribedByService(new List<EventSchema>(), new List<ReadModelSubscription>()),
                new DiscoveryRepository(new DiscoveryClientFactory()),
                statusRepository,
                new MicrowaveConfiguration());

            var consumingServices = await discoveryHandler.GetConsumingServices();

            Assert.IsNotNull(consumingServices);
        }

        [TestMethod]
        public async Task GetConsumingServices_Null()
        {
            var statusRepository = new StatusRepository(EventMongoDb, new EventLocationCache());

            var discoveryHandler = new DiscoveryHandler(
                new ServiceBaseAddressCollection(),
                new EventsSubscribedByService(new List<EventSchema>(), new List<ReadModelSubscription>()),
                new DiscoveryRepository(new DiscoveryClientFactory()),
                statusRepository,
                new MicrowaveConfiguration());

            var consumingServices = await discoveryHandler.GetConsumingServices();

            Assert.IsNotNull(consumingServices);
        }
    }
}