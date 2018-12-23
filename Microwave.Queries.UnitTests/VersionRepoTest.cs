using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;

namespace Microwave.Queries.UnitTests
{
    [TestClass]
    public class VersionRepoTest
    {
        [TestMethod]
        public async Task VersionRepo_DuplicateUpdate()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("VersionRepo_DuplicateUpdate");
            client.DropDatabase("VersionRepo_DuplicateUpdate");

            var versionRepository = new VersionRepository(new ReadModelDatabase(database));

            await versionRepository.SaveVersion(new LastProcessedVersion("Type", 1));
            await versionRepository.SaveVersion(new LastProcessedVersion("Type", 1));

            var count = await versionRepository.GetVersionAsync("Type");
            Assert.AreEqual(1, count);
         }
    }
}