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

            await versionRepository.SaveVersion(new LastProcessedVersion("Type", 1));
            await versionRepository.SaveVersion(new LastProcessedVersion("Type", 1));

            var count = await versionRepository.GetVersionAsync("Type");
            Assert.AreEqual(1, count);
         }
    }
}