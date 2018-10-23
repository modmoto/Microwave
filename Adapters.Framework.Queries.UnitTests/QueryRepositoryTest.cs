using System.Threading.Tasks;
using Adapters.Json.ObjectPersistences;
using Application.Framework;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Adapters.Framework.Queries.UnitTests
{
    [TestFixture]
    public class QueryRepositoryTest
    {
        [Test]
        public async Task UpdateQueryOptimisticConcurrency()
        {
            var options = new DbContextOptionsBuilder<QueryStorageContext>()
                .UseInMemoryDatabase("Add_writes_to_database")
                .Options;

            var queryRepository = new QueryRepository(new QueryStorageContext(options), new ObjectConverter());
            var testQuerry = new TestQuerry { UserName = "Test"};
            await queryRepository.Save(testQuerry);
            var querry1 = await queryRepository.Load<TestQuerry>();
            var querry2 = await queryRepository.Load<TestQuerry>();

            querry2.UserName = "NewName";
            querry1.UserName = "OverwriteName";
            await queryRepository.Save(querry2);
            Assert.ThrowsAsync<IgnoreException>(async () => await queryRepository.Save(querry1));
        }

        [Test]
        public async Task InsertRowVersionWorking()
        {
            var options = new DbContextOptionsBuilder<QueryStorageContext>()
                .UseInMemoryDatabase("Add_writes_to_database")
                .Options;

            var queryRepository = new QueryRepository(new QueryStorageContext(options), new ObjectConverter());
            var testQuery = new TestQuerry { UserName = "Test", Version = 10};
            await queryRepository.Save(testQuery);
            var query = await queryRepository.Load<TestQuerry>();

            Assert.AreEqual("Test", query.UserName);
            Assert.AreEqual(11, query.Version);
        }

        [Test]
        public async Task InsertQuery()
        {
            var options = new DbContextOptionsBuilder<QueryStorageContext>()
                .UseInMemoryDatabase("Add_writes_to_database")
                .Options;

            var queryRepository = new QueryRepository(new QueryStorageContext(options), new ObjectConverter());
            var testQuery = new TestQuerry { UserName = "Test"};
            await queryRepository.Save(testQuery);
            var query = await queryRepository.Load<TestQuerry>();

            Assert.AreEqual("Test", query.UserName);
        }

        [Test]
        public async Task UpdateQuery()
        {
            var options = new DbContextOptionsBuilder<QueryStorageContext>()
                .UseInMemoryDatabase("Add_writes_to_database")
                .Options;

            var queryRepository = new QueryRepository(new QueryStorageContext(options), new ObjectConverter());
            await queryRepository.Save(new TestQuerry { UserName = "Test"});
            await queryRepository.Save(new TestQuerry { UserName = "NewName"});
            var query = await queryRepository.Load<TestQuerry>();

            Assert.AreEqual("NewName", query.UserName);
        }
    }

    public class TestQuerry : Query
    {
        public string UserName { get; set; }
    }
}