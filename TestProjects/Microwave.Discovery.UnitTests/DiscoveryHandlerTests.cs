using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Discovery.EventLocations;
using Microwave.Persistence.MongoDb.Querries;
using Microwave.Persistence.MongoDb.UnitTestsSetup;
using Microwave.WebApi;
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
                new DiscoveryRepository(new DiscoveryClientFactory(new MyMicrowaveHttpClientFactory())),
                statusRepository,
                new DiscoveryConfiguration());

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
                new DiscoveryRepository(new DiscoveryClientFactory(new MyMicrowaveHttpClientFactory())),
                statusRepository,
                new DiscoveryConfiguration());

            var consumingServices = await discoveryHandler.GetConsumingServices();

            Assert.IsNotNull(consumingServices);
        }
    }

    public class MyMicrowaveHttpClientFactory : IMicrowaveHttpClientFactory
    {
        public Task<HttpClient> CreateHttpClient(Uri serviceAdress)
        {
            var discoveryClient = new HttpClient();
            discoveryClient.BaseAddress = new Uri("http://123.de");
            discoveryClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("123");
            return Task.FromResult(discoveryClient);
        }
    }
}