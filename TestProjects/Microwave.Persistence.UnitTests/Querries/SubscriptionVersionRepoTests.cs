using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Discovery.Subscriptions;
using Microwave.Persistence.InMemory.Querries;
using Microwave.Persistence.UnitTestsSetup;
using Microwave.Persistence.UnitTestsSetup.InMemory;
using Microwave.Queries.Ports;

namespace Microwave.Persistence.UnitTests.Querries
{
    [TestClass]
    public class SubscriptionVersionRepoTests
    {
        [TestMethod]
        [PersistenceTypeTest]
        public async Task TestSaveAndStoreVersion(PersistenceLayerProvider layerProvider)
        {
            var subscriptionRepository = layerProvider.SubscriptionRepository;
            var versionRepository = layerProvider.VersionRepository;

            // this is because it is registered as singleton in DI, only ok because of this
            if (layerProvider.GetType() == typeof(InMemoryTestSetup))
            {
                versionRepository = new VersionRepositoryInMemory();
                subscriptionRepository = new SubscriptionRepositoryInMemory(versionRepository);
            }

            await versionRepository.SaveVersionAsync(new LastProcessedVersion("TestEv", DateTimeOffset.Now));
            var subscription = new Subscription("TestEv", new Uri("http://123.de"));
            var currentVersion = subscriptionRepository.GetCurrentVersion(subscription);

            Assert.AreEqual(DateTimeOffset.Now.Day, currentVersion.Result.Day);
        }
    }
}