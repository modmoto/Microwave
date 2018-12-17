using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mongo2Go;
using MongoDB.Driver;

namespace Microwave.Queries.UnitTests
{
    [TestClass]
    public class VersionRepoTest
    {
        [TestMethod]
        public async Task VersionRepo_DuplicateUpdate()
        {
            var runner = MongoDbRunner.Start("VersionRepo_DuplicateUpdate");
            var client = new MongoClient(runner.ConnectionString);
            var database = client.GetDatabase("VersionRepo_DuplicateUpdate");

            var versionRepository = new VersionRepository(new ReadModelDatabase(database));

            await versionRepository.SaveVersion(new LastProcessedVersion("Type", 1));
            await versionRepository.SaveVersion(new LastProcessedVersion("Type", 1));

            var count = await versionRepository.GetVersionAsync("Type");
            Assert.AreEqual(1, count);

            await client.DropDatabaseAsync("VersionRepo_DuplicateUpdate");
            runner.Dispose();
        }
    }
}