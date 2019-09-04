using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Persistence.InMemory.Querries;
using Microwave.Persistence.InMemory.Subscriptions;
using Microwave.Persistence.UnitTestsSetup;
using Microwave.Persistence.UnitTestsSetup.InMemory;
using Microwave.Queries.Ports;
using Microwave.Subscriptions;

namespace Microwave.Persistence.UnitTests.Querries
{
    [TestClass]
    public class VersionRepoTest
    {
        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task VersionRepo_DuplicateUpdate(PersistenceLayerProvider layerProvider)
        {
            var versionRepository = layerProvider.VersionRepository;

            var dateTimeOffset = DateTimeOffset.Now;
            await versionRepository.SaveVersionAsync(new LastProcessedVersion("Type", dateTimeOffset));
            await versionRepository.SaveVersionAsync(new LastProcessedVersion("Type", dateTimeOffset));

            var count = await versionRepository.GetVersionAsync("Type");
            Assert.AreEqual(dateTimeOffset, count);
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task LoadWithNull(PersistenceLayerProvider layerProvider)
        {
            var versionRepository = layerProvider.VersionRepository;
            var count = await versionRepository.GetVersionAsync(null);
            Assert.AreEqual(DateTimeOffset.MinValue, count);
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task VersionRepoRemoteVersion_DuplicateUpdate(PersistenceLayerProvider layerProvider)
        {
            var remoteRepo = layerProvider.RemoteVersionReadRepository;
            var versionRepository = layerProvider.SubscriptionRepository;

            if (layerProvider.GetType() == typeof(InMemoryTestSetup))
            {
                var sharedMemoryClass = new SharedMemoryClass();
                remoteRepo = new RemoteVersionReadRepositoryInMemory(sharedMemoryClass);
                versionRepository = new SubscriptionRepositoryInMemory(layerProvider.VersionRepository, sharedMemoryClass);
            }

            var dateTimeOffset = DateTimeOffset.Now;
            await versionRepository.SaveRemoteVersionAsync(new RemoteVersion("Type", dateTimeOffset));
            await versionRepository.SaveRemoteVersionAsync(new RemoteVersion("Type", dateTimeOffset));

            var count = await remoteRepo.GetVersionAsync("Type");
            Assert.AreEqual(dateTimeOffset, count);
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task VersionRepoRemoteVersion_LoadWithNull(PersistenceLayerProvider layerProvider)
        {
            var versionRepository = layerProvider.RemoteVersionReadRepository;
            var count = await versionRepository.GetVersionAsync(null);
            Assert.AreEqual(DateTimeOffset.MinValue, count);
        }
    }
}