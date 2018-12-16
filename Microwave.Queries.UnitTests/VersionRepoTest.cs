using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microwave.Queries.UnitTests
{
    [TestClass]
    public class VersionRepoTest
    {
        [TestMethod]
        public async Task VersionRepo_SaveAndLoad_UpsertOptionTest()
        {
            var options = new DbContextOptionsBuilder<ReadModelStorageContext>()
                .UseInMemoryDatabase("VersionRepo_SaveAndLoad_UpsertOptionTest")
                .Options;

            var queryStorageContext = new ReadModelStorageContext(options);
            queryStorageContext.Database.EnsureDeleted();
            queryStorageContext.Database.EnsureCreated();
            var versionRepository = new VersionRepository(queryStorageContext);

            await versionRepository.SaveVersion(new LastProcessedVersion("Type", 1));
            await versionRepository.SaveVersion(new LastProcessedVersion("Type", 2));

            var count = await versionRepository.GetVersionAsync("Type");
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public async Task VersionRepo_DuplicateUpdate()
        {
            var options = new DbContextOptionsBuilder<ReadModelStorageContext>()
                .UseInMemoryDatabase("VersionRepo_DuplicateUpdate")
                .Options;

            var versionRepository = new VersionRepository(new ReadModelStorageContext(options));

            await versionRepository.SaveVersion(new LastProcessedVersion("Type", 1));
            await versionRepository.SaveVersion(new LastProcessedVersion("Type", 1));

            var count = await versionRepository.GetVersionAsync("Type");
            Assert.AreEqual(1, count);
        }
    }
}