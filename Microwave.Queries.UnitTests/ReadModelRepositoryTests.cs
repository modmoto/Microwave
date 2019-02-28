using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Application.Exceptions;
using Microwave.Application.Results;
using Microwave.Domain;
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

            var guid = GuidIdentity.Create();
            var testQuerry = new TestReadModel(guid);
            testQuerry.SetVars("Test", new[] {"Jeah", "jeah2"});
            await queryRepository.SaveReadModel(testQuerry);

            var querry1 = await queryRepository.Load<TestReadModel>(guid);

            Assert.AreEqual(guid, querry1.Value.EntityId);
            Assert.AreEqual("Test", querry1.Value.UserName);
            Assert.AreEqual("Jeah", querry1.Value.Strings.First());
        }

        [TestMethod]
        public async Task IdentifiableQuerySaveAndLoadAll()
        {
            var queryRepository = new ReadModelRepository(ReadModelDatabase);

            var testQuerry = new TestReadModel(GuidIdentity.Create());
            var testQuerry2 = new TestReadModel(GuidIdentity.Create());
            testQuerry.SetVars("Test", new[] {"Jeah", "jeah2"});
            testQuerry2.SetVars("Test", new[] {"Jeah", "jeah2"});
            await queryRepository.SaveReadModel(testQuerry);
            await queryRepository.SaveReadModel(testQuerry2);

            var querry1 = await queryRepository.LoadAll<TestReadModel>();

            Assert.AreEqual(2, querry1.Value.Count());
            Assert.AreEqual("Test", querry1.Value.First().UserName);
        }

        [TestMethod]
        public async Task IdentifiableQuerySaveAndLoadAll_UnknownType()
        {
            var queryRepository = new ReadModelRepository(ReadModelDatabase);

            var testQuerry = new TestReadModel(GuidIdentity.Create());
            var testQuerry2 = new TestReadModel(GuidIdentity.Create());
            testQuerry.SetVars("Test", new[] {"Jeah", "jeah2"});
            testQuerry2.SetVars("Test", new[] {"Jeah", "jeah2"});
            await queryRepository.SaveReadModel(testQuerry);
            await queryRepository.SaveReadModel(testQuerry2);

            var loadAll = await queryRepository.LoadAll<TestReadModel2>();
            Assert.IsTrue(loadAll.Is<NotFound>());
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
        public async Task UpdateQuery()
        {
            var queryRepository = new ReadModelRepository(ReadModelDatabase);
            await queryRepository.Save(new TestQuerry { UserName = "Test"});
            await queryRepository.Save(new TestQuerry { UserName = "NewName"});
            var query = (await queryRepository.Load<TestQuerry>()).Value;

            Assert.AreEqual("NewName", query.UserName);
        }

        [TestMethod]
        public async Task LoadTwoTypesOfReadModels_Bug()
        {
            var queryRepository = new ReadModelRepository(ReadModelDatabase);
            var guid2 = GuidIdentity.Create(Guid.NewGuid());
            var testQuery2 = new TestReadModel2(guid2);
            testQuery2.SetVars("Test2", new []{ "Jeah", "jeah2"});

            await queryRepository.SaveReadModel(testQuery2);

            var loadAll2 = await queryRepository.Load<TestReadModel>(guid2);

            Assert.IsTrue(loadAll2.Is<NotFound>());
        }

        [TestMethod]
        public async Task ReadModelNotFoundEceptionHasCorrectT()
        {
            var queryRepository = new ReadModelRepository(ReadModelDatabase);
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
        public TestReadModel(Identity entityId)
        {
            MyId = entityId;
        }

        public Identity MyId { get; set; }

        public string UserName { get; set; }
        public IEnumerable<string> Strings { get; set; } = new List<string>();

        public void SetVars(string test, IEnumerable<string> str)
        {
            UserName = test;
            Strings = str;
        }

        public override Type GetsCreatedOn { get; }
        public override Identity EntityId => MyId;
    }

    public class TestReadModel2 : ReadModel
    {
        public TestReadModel2(GuidIdentity guid2)
        {
            EntityId = guid2;
        }

        public string UserNameAllDifferent { get; private set; }
        public IEnumerable<string> StringsAllDifferent { get; private set; } = new List<string>();

        public void SetVars(string test, IEnumerable<string> str)
        {
            UserNameAllDifferent = test;
            StringsAllDifferent = str;
        }

        public override Type GetsCreatedOn { get; }
        public override Identity EntityId { get; }
    }
}