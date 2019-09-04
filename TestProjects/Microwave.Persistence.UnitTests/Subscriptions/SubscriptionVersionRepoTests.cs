using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Persistence.InMemory.Querries;
using Microwave.Persistence.InMemory.Subscriptions;
using Microwave.Persistence.UnitTestsSetup;
using Microwave.Persistence.UnitTestsSetup.InMemory;
using Microwave.Queries.Ports;
using Microwave.Subscriptions;

namespace Microwave.Persistence.UnitTests.Subscriptions
{
    [TestClass]
    public class SubscriptionVersionRepoTests
    {
        [TestMethod]
        [PersistenceTypeTest]
        public async Task GetCurrentVersion(PersistenceLayerProvider layerProvider)
        {
            var subscriptionRepository = layerProvider.RemoteVersionRepository;
            var versionRepository = layerProvider.VersionRepository;

            // this is because it is registered as singleton in DI, only ok because of this
            if (layerProvider.GetType() == typeof(InMemoryTestSetup))
            {
                versionRepository = new VersionRepositoryInMemory();
                subscriptionRepository = new RemoteVersionRepositoryInMemory(versionRepository, new SharedMemoryClass());
            }

            await versionRepository.SaveVersionAsync(new LastProcessedVersion("TestEv", DateTimeOffset.Now));
            var subscription = new Subscription("TestEv", new Uri("http://123.de"));
            var currentVersion = await subscriptionRepository.GetSubscriptionState(subscription);

            Assert.AreEqual(DateTimeOffset.Now.Day, currentVersion.NewVersion.Day);
        }

        [TestMethod]
        [PersistenceTypeTest]
        public async Task SaveAndGetSubscription(PersistenceLayerProvider layerProvider)
        {
            var subscriptionRepository = layerProvider.SubscriptionRepository;

            var subscribedEvent = "TestEv";
            var subscriberUrl = new Uri("http://123.de");
            var subscription = new Subscription(subscribedEvent, subscriberUrl);
            await subscriptionRepository.StoreSubscriptionAsync(subscription);
            var res = (await subscriptionRepository.LoadSubscriptionsAsync()).ToList();

            Assert.AreEqual(subscribedEvent, res.Single().SubscribedEvent);
            Assert.AreEqual(subscriberUrl, res.Single().SubscriberUrl);
        }
    }
}