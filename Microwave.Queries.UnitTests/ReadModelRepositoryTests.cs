using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Application.Results;
using Microwave.ObjectPersistences;
using Mongo2Go;
using MongoDB.Driver;

namespace Microwave.Queries.UnitTests
{
    [TestClass]
    public class ReadModelRepositoryTests
    {
        [TestMethod]
        public async Task IdentifiableQuerySaveAndLoad()
        {
            var runner = MongoDbRunner.Start("IdentifiableQuerySaveAndLoad");
            var client = new MongoClient(runner.ConnectionString);
            var database = client.GetDatabase("IdentifiableQuerySaveAndLoad");

            var queryRepository = new ReadModelRepository(new ReadModelDatabase(database), new ObjectConverter());

            var guid = Guid.NewGuid();
            var testQuerry = new TestReadModel();
            testQuerry.SetVars("Test", guid, new[] {"Jeah", "jeah2"});
            await queryRepository.Save(new ReadModelWrapper<TestReadModel>(testQuerry, guid, 1));

            var querry1 = (await queryRepository.Load<TestReadModel>(guid)).Value;

            Assert.AreEqual(guid, querry1.Id);
            Assert.AreEqual("Test", querry1.ReadModel.UserName);
            Assert.AreEqual("Jeah", querry1.ReadModel.Strings.First());

            client.DropDatabase("IdentifiableQuerySaveAndLoad");
            runner.Dispose();
        }

        [TestMethod]
        public async Task InsertQuery()
        {
            var runner = MongoDbRunner.Start("InsertQuery");
            var client = new MongoClient(runner.ConnectionString);
            var database = client.GetDatabase("InsertQuery");

            var queryRepository = new ReadModelRepository(new ReadModelDatabase(database), new ObjectConverter());
            var testQuery = new TestQuerry { UserName = "Test"};
            await queryRepository.Save(testQuery);
            var query = (await queryRepository.Load<TestQuerry>()).Value;

            Assert.AreEqual("Test", query.UserName);

            client.DropDatabase("InsertQuery");
            runner.Dispose();
        }

        [TestMethod]
        public async Task GetQuery_WrongType()
        {
            var runner = MongoDbRunner.Start("GetQuery_WrongType");
            var client = new MongoClient(runner.ConnectionString);
            var database = client.GetDatabase("GetQuery_WrongType");
            client.DropDatabase("GetQuery_WrongType");

            var mongoCollection = database.GetCollection<IdentifiableQueryDbo>("IdentifiableQueryDbos");
            await mongoCollection.InsertOneAsync(new IdentifiableQueryDbo
            {
                Id = "6695a111-9aee-44e1-b7cc-94ec5ab5e81b",
                Version = 0,
                Payload =
                    "{\"$type\":\"Microwave.Queries.UnitTests.TestQuerry, Microwave.Queries.UnitTests\",\"UserName\":\"Test\"}",
                QueryType = "TestQuerry"
            });
            var queryRepository = new ReadModelRepository(new ReadModelDatabase(database), new ObjectConverter());

            var result = await queryRepository.Load<TestReadModel>(new Guid("6695a111-9aee-44e1-b7cc-94ec5ab5e81b"));

            Assert.IsTrue(result.Is<NotFound>());

            client.DropDatabase("GetQuery_WrongType");
            runner.Dispose();
        }

        [TestMethod]
        public async Task InsertQuery_ConcurrencyProblem()
        {
            var runner = MongoDbRunner.Start("InsertQuery_ConcurrencyProblem");
            var client = new MongoClient(runner.ConnectionString);
            var database = client.GetDatabase("InsertQuery_ConcurrencyProblem");

            var queryRepository = new ReadModelRepository(new ReadModelDatabase(database), new ObjectConverter());
            var testQuery = new TestQuerry { UserName = "Test1"};
            var testQuery2 = new TestQuerry { UserName = "Test2"};
            var save = queryRepository.Save(testQuery);
            var save2 = queryRepository.Save(testQuery2);

            await Task.WhenAll(new List<Task> { save, save2});

            client.DropDatabase("InsertQuery_ConcurrencyProblem");
            runner.Dispose();
        }

        [TestMethod]
        public async Task InsertIDQuery_ConcurrencyProblem()
        {
            var runner = MongoDbRunner.Start("InsertIDQuery_ConcurrencyProblem");
            var client = new MongoClient(runner.ConnectionString);
            var database = client.GetDatabase("InsertIDQuery_ConcurrencyProblem");

            var queryRepository = new ReadModelRepository(new ReadModelDatabase(database), new ObjectConverter());
            Guid guid = Guid.NewGuid();
            var testQuery = new TestReadModel();
            testQuery.SetVars("Test1", guid, new []{ "Jeah", "jeah2"});
            var testQuery2 = new TestReadModel();
            testQuery2.SetVars("Test2", guid, new []{ "Jeah", "jeah2"});

            var save = queryRepository.Save(new ReadModelWrapper<TestReadModel>(testQuery, guid, 1));
            var save2 = queryRepository.Save(new ReadModelWrapper<TestReadModel>(testQuery2, guid, 2));

            await Task.WhenAll(new List<Task<Result>> { save, save2 });

            var resultOfLoad = await queryRepository.Load<TestReadModel>(guid);
            Assert.AreEqual(2, resultOfLoad.Value.Version);

            client.DropDatabase("InsertIDQuery_ConcurrencyProblem");
            runner.Dispose();
        }

        [TestMethod]
        public async Task UpdateQuery()
        {
            var runner = MongoDbRunner.Start("UpdateQuery");
            var client = new MongoClient(runner.ConnectionString);
            var database = client.GetDatabase("UpdateQuery");

            var queryRepository = new ReadModelRepository(new ReadModelDatabase(database), new ObjectConverter());
            await queryRepository.Save(new TestQuerry { UserName = "Test"});
            await queryRepository.Save(new TestQuerry { UserName = "NewName"});
            var query = (await queryRepository.Load<TestQuerry>()).Value;

            Assert.AreEqual("NewName", query.UserName);

            client.DropDatabase("UpdateQuery");
            runner.Dispose();
        }

        [TestMethod]
        public async Task LoadAllReadModels()
        {
            var runner = MongoDbRunner.Start("LoadAllReadModels");
            var client = new MongoClient(runner.ConnectionString);
            var database = client.GetDatabase("LoadAllReadModels");
            client.DropDatabase("LoadAllReadModels");

            var queryRepository = new ReadModelRepository(new ReadModelDatabase(database), new ObjectConverter());
            Guid guid = Guid.NewGuid();
            Guid guid2 = Guid.NewGuid();
            var testQuery = new TestReadModel();
            testQuery.SetVars("Test1", guid, new []{ "Jeah", "jeah2"});
            var testQuery2 = new TestReadModel();
            testQuery2.SetVars("Test2", guid2, new []{ "Jeah", "jeah2"});

            await queryRepository.Save(new ReadModelWrapper<TestReadModel>(testQuery, guid, 1));
            await queryRepository.Save(new ReadModelWrapper<TestReadModel>(testQuery2, guid2, 1));

            var loadAll = await queryRepository.LoadAll<TestReadModel>();
            var readModelWrappers = loadAll.Value.ToList();

            Assert.AreEqual(2, readModelWrappers.Count);
            Assert.AreEqual(testQuery.UserName, readModelWrappers[0].ReadModel.UserName);
            Assert.AreEqual(testQuery2.UserName, readModelWrappers[1].ReadModel.UserName);

            runner.Dispose();
        }
    }

    public class TestQuerry : Query
    {
        public string UserName { get; set; }
    }

    public class TestReadModel : ReadModel
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