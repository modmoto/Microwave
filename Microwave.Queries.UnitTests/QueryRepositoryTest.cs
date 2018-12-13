using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Application.Results;
using Microwave.ObjectPersistences;

namespace Microwave.Queries.UnitTests
{
    [TestClass]
    public class QueryRepositoryTest
    {
        [TestMethod]
        public async Task IdentifiableQuerySaveAndLoad()
        {
            var options = new DbContextOptionsBuilder<QueryStorageContext>()
                .UseInMemoryDatabase("IdentifiableQuerySaveAndLoad")
                .Options;

            var queryRepository = new QueryRepository(new QueryStorageContext(options), new ObjectConverter());
            var guid = Guid.NewGuid();
            var testQuerry = new TestIdQuerry();
            testQuerry.SetVars("Test", guid, new []{ "Jeah", "jeah2"});
            await queryRepository.Save(new ReadModelWrapper<TestIdQuerry>(testQuerry, guid, 1));

            var querry1 = (await queryRepository.Load<TestIdQuerry>(guid)).Value;

            Assert.AreEqual(guid, querry1.Id);
            Assert.AreEqual("Test", querry1.ReadModel.UserName);
            Assert.AreEqual("Jeah", querry1.ReadModel.Strings.First());
        }

        [TestMethod]
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

        [TestMethod]
        public async Task GetQuery_WrongType()
        {
            var options = new DbContextOptionsBuilder<QueryStorageContext>()
                .UseInMemoryDatabase("GetQuery_WrongType")
                .Options;

            var queryStorageContext = new QueryStorageContext(options);
            var identifiableQueryDbo = new IdentifiableQueryDbo
            {
                Id = "6695a111-9aee-44e1-b7cc-94ec5ab5e81b",
                Version = 0,
                Payload = "{\"$type\":\"Microwave.Queries.UnitTests.TestQuerry, Microwave.Queries.UnitTests\",\"UserName\":\"Test\"}",
                QueryType = "TestQuerry"
            };
            queryStorageContext.IdentifiableQuerries.Add(identifiableQueryDbo);
            await queryStorageContext.SaveChangesAsync();
            var queryRepository = new QueryRepository(queryStorageContext, new ObjectConverter());

            var result = await queryRepository.Load<TestIdQuerry>(new Guid("6695a111-9aee-44e1-b7cc-94ec5ab5e81b"));

            Assert.IsTrue(result.Is<NotFound>());
        }

        [TestMethod]
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

        [TestMethod]
        public async Task InsertIDQuery_ConcurrencyProblem()
        {
            var options = new DbContextOptionsBuilder<QueryStorageContext>()
                .UseInMemoryDatabase("InsertIDQuery_ConcurrencyProblem")
                .Options;

            var queryRepository = new QueryRepository(new QueryStorageContext(options), new ObjectConverter());
            Guid guid = Guid.NewGuid();
            var testQuery = new TestIdQuerry();
            testQuery.SetVars("Test1", guid, new []{ "Jeah", "jeah2"});
            var testQuery2 = new TestIdQuerry();
            testQuery2.SetVars("Test2", guid, new []{ "Jeah", "jeah2"});

            var save = queryRepository.Save(new ReadModelWrapper<TestIdQuerry>(testQuery, guid, 1));
            var save2 = queryRepository.Save(new ReadModelWrapper<TestIdQuerry>(testQuery2, guid, 2));

            await Task.WhenAll(new List<Task> { save, save2 });

            var result = await queryRepository.Load<TestIdQuerry>(guid);
            Assert.AreEqual(2, result.Value.Version);
        }

        [TestMethod]
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

    public class TestIdQuerry : ReadModel
    {
        public string UserName { get; private set; }
        public IEnumerable<string> Strings { get; private set; } = new List<string>();

        public void SetVars(string test, Guid guid, IEnumerable<string> str)
        {
            UserName = test;
            Strings = str;
        }
    }
}