using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Persistence.UnitTestsSetup;
using Microwave.Queries.Ports;

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
            var versionRepository = layerProvider.VersionRepository;

            var dateTimeOffset = DateTimeOffset.Now;
            await versionRepository.SaveRemoteVersionAsync(new LastProcessedVersion("Type", dateTimeOffset));
            await versionRepository.SaveRemoteVersionAsync(new LastProcessedVersion("Type", dateTimeOffset));

            var count = await versionRepository.GetRemoteVersionAsync("Type");
            Assert.AreEqual(dateTimeOffset, count);
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task VersionRepoRemoteVersion_LoadWithNull(PersistenceLayerProvider layerProvider)
        {
            var versionRepository = layerProvider.VersionRepository;
            var count = await versionRepository.GetRemoteVersionAsync(null);
            Assert.AreEqual(DateTimeOffset.MinValue, count);
        }
    }
}