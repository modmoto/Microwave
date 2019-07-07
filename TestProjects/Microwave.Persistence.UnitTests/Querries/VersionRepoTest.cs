using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Persistence.UnitTestSetupPorts;
using Microwave.Queries;

namespace Microwave.Persistence.UnitTests.Querries
{
    [TestClass]
    public class VersionRepoTest
    {
        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task VersionRepo_DuplicateUpdate(IPersistenceLayerProvider layerProvider)
        {
            var versionRepository = layerProvider.VersionRepository;

            var dateTimeOffset = DateTimeOffset.Now;
            await versionRepository.SaveVersion(new LastProcessedVersion("Type", dateTimeOffset));
            await versionRepository.SaveVersion(new LastProcessedVersion("Type", dateTimeOffset));

            var count = await versionRepository.GetVersionAsync("Type");
            Assert.AreEqual(dateTimeOffset, count);
        }
    }
}