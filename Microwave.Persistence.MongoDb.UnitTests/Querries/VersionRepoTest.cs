using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Persistence.MongoDb.Querries;
using Microwave.Persistence.MongoDb.UnitTests.Eventstores;
using Microwave.Queries;

namespace Microwave.Persistence.MongoDb.UnitTests.Querries
{
    [TestClass]
    public class VersionRepoTest : IntegrationTests
    {
        [TestMethod]
        public async Task VersionRepo_DuplicateUpdate()
        {
            var versionRepository = new VersionRepository(EventDatabase);

            var dateTimeOffset = DateTimeOffset.Now;
            await versionRepository.SaveVersion(new LastProcessedVersion("Type", dateTimeOffset));
            await versionRepository.SaveVersion(new LastProcessedVersion("Type", dateTimeOffset));

            var count = await versionRepository.GetVersionAsync("Type");
            Assert.AreEqual(dateTimeOffset, count);
         }
    }
}