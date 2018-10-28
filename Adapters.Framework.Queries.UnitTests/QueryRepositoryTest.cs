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
        public async Task UpdateQueryOptimisticConcurrency()
        {
            var options = new DbContextOptionsBuilder<QueryStorageContext>()
                .UseInMemoryDatabase("UpdateQueryOptimisticConcurrency")
                .Options;

            var queryRepository = new QueryRepository(new QueryStorageContext(options), new ObjectConverter());
            var testQuerry = new TestQuerry { UserName = "Test"};
            await queryRepository.Save(testQuerry);

            var querry1 = (await queryRepository.Load<TestQuerry>()).Value;
            var querry2 = (await queryRepository.Load<TestQuerry>()).Value;

            querry1.UserName = "OverwriteName";
            querry2.UserName = "NewName";

            var result1 = queryRepository.Save(querry1);
            var result = queryRepository.Save(querry2);

            var allResults = await Task.WhenAll(result, result1);

            Assert.Throws<ConcurrencyException>(() => CheckAllResults(allResults));
        }

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
        public async Task ContextTest()
        {
            var options = new DbContextOptionsBuilder<QueryStorageContext>()
                .UseInMemoryDatabase("contextTest")
                .Options;

            using (var db =  new QueryStorageContext(options))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                db.Querries.Add(new QueryDbo { Version = 0, Type = "Test", Payload = "LoadOld"});
                db.SaveChanges();
            }

            var t1 = Task.Run(() =>
            {
                using (var db = new QueryStorageContext(options))
                {
                    var existing = db.Querries.Find("Test");
                    existing.Payload = "yyy";
                    existing.Version = 1;
                    db.SaveChanges();
                }
            });

            var t2 = Task.Run(() =>
            {
                using (var db = new QueryStorageContext(options))
                {
                    var existing = db.Querries.Find("Test");
                    existing.Payload = "zzz";
                    existing.Version = 1;
                    db.SaveChanges();
                }
            });

            var aggregateException = Assert.Throws<AggregateException>(() => Task.WaitAll(t1, t2));
            Assert.AreEqual(typeof(DbUpdateConcurrencyException), aggregateException.InnerExceptions.ToList()[0].GetType());
        }

        [Test]
        public async Task InsertRowVersionWorking()
        {
            var options = new DbContextOptionsBuilder<QueryStorageContext>()
                .UseInMemoryDatabase("InsertRowVersionWorking")
                .Options;

            var queryRepository = new QueryRepository(new QueryStorageContext(options), new ObjectConverter());
            var testQuery = new TestQuerry { UserName = "Test"};
            await queryRepository.Save(testQuery);
            var query = (await queryRepository.Load<TestQuerry>()).Value;

            Assert.AreEqual("Test", query.UserName);
            Assert.AreEqual(0, query.Version);
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
            await queryRepository.Save(new TestQuerry { UserName = "Test", Version = -1});
            await queryRepository.Save(new TestQuerry { UserName = "NewName", Version = 0});
            var query = (await queryRepository.Load<TestQuerry>()).Value;

            Assert.AreEqual("NewName", query.UserName);
            Assert.AreEqual(1, query.Version);
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