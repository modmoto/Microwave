using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adapters.Json.ObjectPersistences;
using Microsoft.EntityFrameworkCore;
using Microwave.Application;
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
        public async Task InsertQuery_ConcurrencyProblem()
        {
            var options = new DbContextOptionsBuilder<QueryStorageContext>()
                .UseInMemoryDatabase("InsertQuery_ConcurrencyProblem")
                .Options;

            var queryRepository = new QueryRepository(new QueryStorageContext(options), new ObjectConverter());
            var testQuery = new TestQuerry { UserName = "Test1"};
            var testQuery2 = new TestQuerry { UserName = "Test2"};
            var save = queryRepository.Save(testQuery);
            var save2 = queryRepository.Save(testQuery2);

            await Task.WhenAll(new List<Task> { save, save2});
        }

        [Test]
        public async Task InsertIDQuery_ConcurrencyProblem()
        {
            var options = new DbContextOptionsBuilder<QueryStorageContext>()
                .UseInMemoryDatabase("InsertIDQuery_ConcurrencyProblem")
                .Options;

            var queryRepository = new QueryRepository(new QueryStorageContext(options), new ObjectConverter());
            Guid guid = Guid.NewGuid();
            var testQuery = new TestIdQuerry { Id = guid, UserName = "Test1"};
            var testQuery2 = new TestIdQuerry { Id = guid, UserName = "Test2"};
            var save = queryRepository.Save(testQuery);
            var save2 = queryRepository.Save(testQuery2);

            await Task.WhenAll(new List<Task> { save, save2});
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