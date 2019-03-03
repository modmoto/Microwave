using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Eventstores.UnitTests;

namespace Microwave.Queries.UnitTests
{
    [TestClass]
    public class VersionRepoTest : IntegrationTests
    {
        [TestMethod]
        public async Task VersionRepo_DuplicateUpdate()
        {
            var versionRepository = new VersionRepository(ReadModelDatabase);

            var dateTimeOffset = DateTimeOffset.Now;
            await versionRepository.SaveVersion(new LastProcessedVersion("Type", dateTimeOffset));
            await versionRepository.SaveVersion(new LastProcessedVersion("Type", dateTimeOffset));

            var count = await versionRepository.GetVersionAsync("Type");
            Assert.AreEqual(dateTimeOffset, count);
         }
    }
}