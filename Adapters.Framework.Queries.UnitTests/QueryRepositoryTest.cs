using System;
using System.Linq;
using System.Threading.Tasks;
using Adapters.Json.ObjectPersistences;
using Application.Framework;
using Application.Framework.Exceptions;
using Application.Framework.Results;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Adapters.Framework.Queries.UnitTests
{
    [TestFixture]
    public class QueryRepositoryTest
    {
        [Test]
        public async Task IdentifiableQuerySaveAndLoad()
        {
            var options = new DbContextOptionsBuilder<QueryStorageContext>()
                .UseInMemoryDatabase("IdentifiableQuerySaveAndLoad")
                .Options;

            var queryRepository = new QueryRepository(new QueryStorageContext(options), new ObjectConverter());
            var guid = Guid.NewGuid();
            var testQuerry = new TestIdQuerry { UserName = "Test", Id = guid};
            await queryRepository.Save(testQuerry);

            var querry1 = (await queryRepository.Load<TestIdQuerry>(guid)).Value;

            Assert.AreEqual(guid, querry1.Id);
            Assert.AreEqual("Test", querry1.UserName);
        }

        private static void CheckAllResults(Result[] whenAll)
        {
            foreach (var result in whenAll)
            {
                result.Check();
            }
        }

        [Test]
        public async Task InsertQuery()
        {
            var options = new DbContextOptionsBuilder<QueryStorageContext>()
                .UseInMemoryDatabase("InsertQuery")
                .Options;

            var queryRepository = new QueryRepository(new QueryStorageContext(options), new ObjectConverter());
            var testQuery = new TestQuerry { UserName = "Test"};
            await queryRepository.Save(testQuery);
            var query = (await queryRepository.Load<TestQuerry>()).Value;

            Assert.AreEqual("Test", query.UserName);
        }

        [Test]
        public async Task UpdateQuery()
        {
            var options = new DbContextOptionsBuilder<QueryStorageContext>()
                .UseInMemoryDatabase("UpdateQuery")
                .Options;

            var queryRepository = new QueryRepository(new QueryStorageContext(options), new ObjectConverter());
            await queryRepository.Save(new TestQuerry { UserName = "Test"});
            await queryRepository.Save(new TestQuerry { UserName = "NewName"});
            var query = (await queryRepository.Load<TestQuerry>()).Value;

            Assert.AreEqual("NewName", query.UserName);
        }
    }

    public class TestQuerry : Query
    {
        public string UserName { get; set; }
    }

    public class TestIdQuerry : IdentifiableQuery
    {
        public string UserName { get; set; }
    }
}