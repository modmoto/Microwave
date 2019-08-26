using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Discovery.EventLocations;
using Microwave.Discovery.ServiceMaps;
using Microwave.Discovery.Subscriptions;
using Microwave.Persistence.MongoDb.Querries;
using Microwave.Persistence.UnitTestsSetup.MongoDb;
using Microwave.WebApi;
using Microwave.WebApi.Discovery;
using Moq;

namespace Microwave.Discovery.UnitTests
{
    [TestClass]
    public class DiscoveryHandlerTests : IntegrationTests
    {
        private ISubscriptionRepository _subscriptionRepository;

        [TestInitialize]
        public void Setup()
        {
            var mock = new Mock<ISubscriptionRepository>();
            _subscriptionRepository = mock.Object;
        }

        [TestMethod]
        public async Task GetConsumingServices()
        {
            var statusRepository = new StatusRepositoryMongoDb(EventMongoDb, new EventLocationCache());
            await statusRepository.SaveEventLocation(
                new EventLocation(
                    new List<EventsPublishedByService>(),
                    new EventsSubscribedByService(
                        new List<EventSchema>(),
                        new List<ReadModelSubscription>())
            ));

            var discoveryHandler = new DiscoveryHandler(
                new ServiceBaseAddressCollection(),
                new EventsSubscribedByService(new List<EventSchema>(), new List<ReadModelSubscription>()),
                null,
                new DiscoveryRepository(new DiscoveryClientFactory(new MyMicrowaveHttpClientFactory())),
                statusRepository,
                _subscriptionRepository,
                new DiscoveryConfiguration());

            var consumingServices = await discoveryHandler.GetConsumingServices();

            Assert.IsNotNull(consumingServices);
        }

        [TestMethod]
        public async Task GetConsumingServices_Null()
        {
            var statusRepository = new StatusRepositoryMongoDb(EventMongoDb, new EventLocationCache());

            var discoveryHandler = new DiscoveryHandler(
                new ServiceBaseAddressCollection(),
                new EventsSubscribedByService(new List<EventSchema>(), new List<ReadModelSubscription>()),
                null,
                new DiscoveryRepository(new DiscoveryClientFactory(new MyMicrowaveHttpClientFactory())),
                statusRepository,
                _subscriptionRepository,
                new DiscoveryConfiguration());

            var consumingServices = await discoveryHandler.GetConsumingServices();

            Assert.IsNotNull(consumingServices);
        }

        [TestMethod]
        public async Task GetPublishedEvents()
        {
            var discoveryHandler = new DiscoveryHandler(
                null,
                null,
                EventsPublishedByService.Reachable(
                    new ServiceEndPoint(null),
                    new List<EventSchema>()),
                null,
                null,
                _subscriptionRepository,
                null);

            var events = await discoveryHandler.GetPublishedEvents();

            Assert.IsNotNull(events);
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