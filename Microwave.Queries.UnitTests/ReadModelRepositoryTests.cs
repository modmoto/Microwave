using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Application.Exceptions;
using Microwave.Application.Results;
using Microwave.Eventstores.UnitTests;

namespace Microwave.Queries.UnitTests
{
    [TestClass]
    public class ReadModelRepositoryTests : IntegrationTests
    {
        [TestMethod]
        public async Task IdentifiableQuerySaveAndLoad()
        {
            var queryRepository = new ReadModelRepository(ReadModelDatabase);

            var guid = Guid.NewGuid();
            var testQuerry = new TestReadModel();
            testQuerry.SetVars("Test", new[] {"Jeah", "jeah2"});
            await queryRepository.Save(new ReadModelWrapper<TestReadModel>(testQuerry, guid.ToString(), 1));

            var querry1 = (await queryRepository.Load<TestReadModel>(guid.ToString())).Value;

            Assert.AreEqual(guid.ToString(), querry1.Id);
            Assert.AreEqual("Test", querry1.ReadModel.UserName);
            Assert.AreEqual("Jeah", querry1.ReadModel.Strings.First());
        }

        [TestMethod]
        public async Task InsertQuery()
        {
            var queryRepository = new ReadModelRepository(ReadModelDatabase);
            var testQuery = new TestQuerry { UserName = "Test"};
            await queryRepository.Save(testQuery);
            var query = (await queryRepository.Load<TestQuerry>()).Value;

            Assert.AreEqual("Test", query.UserName);
        }

        [TestMethod]
        public async Task InsertQuery_ConcurrencyProblem()
        {
            var queryRepository = new ReadModelRepository(ReadModelDatabase);
            var testQuery = new TestQuerry { UserName = "Test1"};
            var testQuery2 = new TestQuerry { UserName = "Test2"};
            var save = queryRepository.Save(testQuery);
            var save2 = queryRepository.Save(testQuery2);

            await Task.WhenAll(new List<Task> { save, save2 });
        }

        [TestMethod]
        public async Task InsertIDQuery_ConcurrencyProblem()
        {
            var queryRepository = new ReadModelRepository(ReadModelDatabase);
            var guid = Guid.NewGuid();
            var testQuery = new TestReadModel();
            testQuery.SetVars("Test1", new []{ "Jeah", "jeah2"});
            var testQuery2 = new TestReadModel();
            testQuery2.SetVars("Test2", new []{ "Jeah", "jeah2"});

            var save = queryRepository.Save(new ReadModelWrapper<TestReadModel>(testQuery, guid.ToString(), 1));
            var save2 = queryRepository.Save(new ReadModelWrapper<TestReadModel>(testQuery2, guid.ToString(), 2));

            await Task.WhenAll(new List<Task<Result>> { save, save2 });

            var resultOfLoad = await queryRepository.Load<TestReadModel>(guid.ToString());
            Assert.AreEqual(2, resultOfLoad.Value.Version);
        }

        [TestMethod]
        public async Task UpdateQuery()
        {
            var queryRepository = new ReadModelRepository(ReadModelDatabase);
            await queryRepository.Save(new TestQuerry { UserName = "Test"});
            await queryRepository.Save(new TestQuerry { UserName = "NewName"});
            var query = (await queryRepository.Load<TestQuerry>()).Value;

            Assert.AreEqual("NewName", query.UserName);
        }

        [TestMethod]
        public async Task LoadAllReadModels()
        {
            var queryRepository = new ReadModelRepository(ReadModelDatabase);
            var guid = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            var testQuery = new TestReadModel();
            testQuery.SetVars("Test1", new []{ "Jeah", "jeah2"});
            var testQuery2 = new TestReadModel();
            testQuery2.SetVars("Test2", new []{ "Jeah", "jeah2"});

            await queryRepository.Save(new ReadModelWrapper<TestReadModel>(testQuery, guid.ToString(), 1));
            await queryRepository.Save(new ReadModelWrapper<TestReadModel>(testQuery2, guid2.ToString(), 1));

            var loadAll = await queryRepository.LoadAll<TestReadModel>();
            var readModelWrappers = loadAll.Value.ToList();

            Assert.AreEqual(2, readModelWrappers.Count);
            Assert.AreEqual(testQuery.UserName, readModelWrappers[0].ReadModel.UserName);
            Assert.AreEqual(testQuery2.UserName, readModelWrappers[1].ReadModel.UserName);
        }

        [TestMethod]
        public async Task LoadTwoTypesOfReadModels_Bug()
        {
            var queryRepository = new ReadModelRepository(ReadModelDatabase);
            var guid2 = Guid.NewGuid();
            var testQuery2 = new TestReadModel2();
            testQuery2.SetVars("Test2", new []{ "Jeah", "jeah2"});

            await queryRepository.Save(new ReadModelWrapper<TestReadModel2>(testQuery2, guid2.ToString(), 1));

            var loadAll2 = await queryRepository.Load<TestReadModel>(guid2.ToString());

            Assert.IsTrue(loadAll2.Is<NotFound>());
        }

        [TestMethod]
        public async Task ReadModelNotFoundEceptionHasCorrectT()
        {
            var queryRepository = new ReadModelRepository(ReadModelDatabase);
            var guid2 = Guid.NewGuid();
            var result = await queryRepository.Load<TestReadModel>(guid2.ToString());

            var notFoundException = Assert.ThrowsException<NotFoundException>(() => result.Value);
            Assert.IsTrue(notFoundException.Message.StartsWith("Could not find TestReadModel"));
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

        public void SetVars(string test, IEnumerable<string> str)
        {
            UserName = test;
            Strings = str;
        }

        public override Type GetsCreatedOn { get; }
    }

    public class TestReadModel2 : ReadModel
    {
        public string UserNameAllDifferent { get; private set; }
        public IEnumerable<string> StringsAllDifferent { get; private set; } = new List<string>();

        public void SetVars(string test, IEnumerable<string> str)
        {
            UserNameAllDifferent = test;
            StringsAllDifferent = str;
        }

        public override Type GetsCreatedOn { get; }
    }
}