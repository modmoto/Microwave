using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain;
using Microwave.Domain.Exceptions;
using Microwave.Domain.Results;
using Microwave.Persistence.MongoDb.Querries;
using Microwave.Persistence.MongoDb.UnitTests.Eventstores;
using Microwave.Queries;

namespace Microwave.Persistence.MongoDb.UnitTests.Querries
{
    [TestClass]
    public class ReadModelRepositoryTests : IntegrationTests
    {
        [TestMethod]
        public async Task IdentifiableQuerySaveAndLoad()
        {
            var queryRepository = new ReadModelRepository(EventDatabase);

            var guid = GuidIdentity.Create(Guid.NewGuid());
            var testQuerry = new TestReadModel();
            testQuerry.SetVars("Test", new[] {"Jeah", "jeah2"});
            await queryRepository.Save(ReadModelResult<TestReadModel>.Ok(testQuerry, guid, 1));

            var querry1 = await queryRepository.Load<TestReadModel>(guid);

            Assert.AreEqual(guid, querry1.Id);
            Assert.AreEqual("Test", querry1.Value.UserName);
            Assert.AreEqual("Jeah", querry1.Value.Strings.First());
        }

        [TestMethod]
        public async Task IdentifiableQuerySaveAndLoadAll()
        {
            var queryRepository = new ReadModelRepository(EventDatabase);

            var testQuerry = new TestReadModel();
            var testQuerry2 = new TestReadModel();
            testQuerry.SetVars("Test", new[] {"Jeah", "jeah2"});
            testQuerry2.SetVars("Test", new[] {"Jeah", "jeah2"});
            await queryRepository.Save(ReadModelResult<TestReadModel>.Ok(testQuerry, GuidIdentity.Create(), 1));
            await queryRepository.Save(ReadModelResult<TestReadModel>.Ok(testQuerry2, GuidIdentity.Create(), 1));

            var querry1 = await queryRepository.LoadAll<TestReadModel>();

            Assert.AreEqual(2, querry1.Value.Count());
            Assert.AreEqual("Test", querry1.Value.First().UserName);
        }

        [TestMethod]
        public async Task IdentifiableQuerySaveAndLoadAll_UnknownType()
        {
            var queryRepository = new ReadModelRepository(EventDatabase);

            var testQuerry = new TestReadModel();
            var testQuerry2 = new TestReadModel();
            testQuerry.SetVars("Test", new[] {"Jeah", "jeah2"});
            testQuerry2.SetVars("Test", new[] {"Jeah", "jeah2"});
            await queryRepository.Save(ReadModelResult<TestReadModel>.Ok(testQuerry, GuidIdentity.Create(), 1));
            await queryRepository.Save(ReadModelResult<TestReadModel>.Ok(testQuerry2, GuidIdentity.Create(), 1));

            var loadAll = await queryRepository.LoadAll<TestReadModel2>();
            Assert.IsTrue(loadAll.Is<NotFound>());
        }

        [TestMethod]
        public async Task InsertQuery()
        {
            var queryRepository = new ReadModelRepository(EventDatabase);
            var testQuery = new TestQuerry { UserName = "Test"};
            await queryRepository.Save(testQuery);
            var query = (await queryRepository.Load<TestQuerry>()).Value;

            Assert.AreEqual("Test", query.UserName);
        }

        [TestMethod]
        public async Task InsertQuery_ConcurrencyProblem()
        {
            var queryRepository = new ReadModelRepository(EventDatabase);
            var testQuery = new TestQuerry { UserName = "Test1"};
            var testQuery2 = new TestQuerry { UserName = "Test2"};
            var save = queryRepository.Save(testQuery);
            var save2 = queryRepository.Save(testQuery2);

            await Task.WhenAll(new List<Task> { save, save2 });
        }

        [TestMethod]
        public async Task UpdateQuery()
        {
            var queryRepository = new ReadModelRepository(EventDatabase);
            await queryRepository.Save(new TestQuerry { UserName = "Test"});
            await queryRepository.Save(new TestQuerry { UserName = "NewName"});
            var query = (await queryRepository.Load<TestQuerry>()).Value;

            Assert.AreEqual("NewName", query.UserName);
        }

        [TestMethod]
        public async Task LoadTwoTypesOfReadModels_Bug()
        {
            var queryRepository = new ReadModelRepository(EventDatabase);
            var guid2 = GuidIdentity.Create(Guid.NewGuid());
            var testQuery2 = new TestReadModel2();
            testQuery2.SetVars("Test2", new []{ "Jeah", "jeah2"});

            await queryRepository.Save(new ReadModelResult<TestReadModel2>(testQuery2, guid2, 1));

            var loadAll2 = await queryRepository.Load<TestReadModel>(guid2);

            Assert.IsTrue(loadAll2.Is<NotFound>());
        }

        [TestMethod]
        public async Task ReadModelNotFoundEceptionHasCorrectT()
        {
            var queryRepository = new ReadModelRepository(EventDatabase);
            var guid2 = GuidIdentity.Create(Guid.NewGuid());
            var result = await queryRepository.Load<TestReadModel>(guid2);

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