using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain.EventSourcing;
using Microwave.Domain.Identities;
using Microwave.EventStores;
using Microwave.Persistence.MongoDb.Eventstores;
using Microwave.Persistence.MongoDb.UnitTestsSetup;
using MongoDB.Driver;

namespace Microwave.Eventstores.UnitTests
{
    [TestClass]
    public class SnapshotTests  : IntegrationTests
    {
        [TestMethod]
        public async Task SnapshotRealized()
        {
            var mongoCollection = EventMongoDb.Database.GetCollection<SnapShotDbo<User>>("SnapShotDbos");

            var repo = new EventRepository(EventMongoDb, new VersionCache(EventMongoDb));
            var eventStore = new EventStore(repo, new SnapShotRepository(EventMongoDb));

            var entityId = GuidIdentity.Create(Guid.NewGuid());
            await eventStore.AppendAsync(new List<IDomainEvent>
            {
                new Event1(entityId),
                new Event2(entityId, "Peter")
            }, 0);

            await eventStore.LoadAsync<User>(entityId);

            var snapShotDboOld = (await mongoCollection.FindAsync(e => e.Id == entityId.ToString())).ToList()
            .FirstOrDefault();

            Assert.IsNull(snapShotDboOld);

            await eventStore.AppendAsync(new List<IDomainEvent>
            {
                new Event3(entityId, 14),
                new Event2(entityId, "PeterNeu")
            }, 2);

            var eventstoreResult = await eventStore.LoadAsync<User>(entityId);

            var user = eventstoreResult;
            Assert.AreEqual(4, eventstoreResult.Version);
            Assert.AreEqual(14, user.Value.Age);
            Assert.AreEqual("PeterNeu", user.Value.Name);
            Assert.AreEqual(entityId.Id, user.Value.Id.Id);

            var snapShotDbo = (await mongoCollection.FindAsync(e => e.Id == entityId.Id)).ToList().First();

            Assert.AreEqual(4, snapShotDbo.Version);
            Assert.AreEqual(entityId.Id, snapShotDbo.Id);
            var userSnapShot = snapShotDbo.Payload;

            Assert.AreEqual(14, userSnapShot.Age);
            Assert.AreEqual("PeterNeu", userSnapShot.Name);
            Assert.AreEqual(entityId.Id, userSnapShot.Id.Id);
        }

        [TestMethod]
        public async Task SnapshotExactlyOnSnapShotTime_DoesNotReturnNotFoundBug()
        {
            var repo = new EventRepository(EventMongoDb, new VersionCache(EventMongoDb));
            var eventStore = new EventStore(repo, new SnapShotRepository(EventMongoDb));

            var entityId = GuidIdentity.Create(Guid.NewGuid());
            await eventStore.AppendAsync(new List<IDomainEvent>
            {
                new Event1(entityId),
                new Event2(entityId, "Peter"),
                new Event2(entityId, "Peterneu")
            }, 0);

            await eventStore.LoadAsync<User>(entityId);
            var result = await eventStore.LoadAsync<User>(entityId);

            Assert.AreEqual("Peterneu", result.Value.Name);
            Assert.AreEqual(3, result.Version);
        }
    }

    [SnapShotAfter(3)]
    public class User : Entity, IApply<Event1>, IApply<Event2>, IApply<Event3>
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public Identity Id { get; set; }

        public void Apply(Event1 domainEvent)
        {
            Id = domainEvent.EntityId;
        }

        public void Apply(Event2 domainEvent)
        {
            Name = domainEvent.Name;
        }

        public void Apply(Event3 domainEvent)
        {
            Age = domainEvent.Age;
        }
    }

    public class Event1 : IDomainEvent
    {
        public Event1(Identity entityId)
        {
            EntityId = entityId;
        }

        public Identity EntityId { get; }
    }

    public class Event2 : IDomainEvent
    {
        public Event2(Identity entityId, string name)
        {
            EntityId = entityId;
            Name = name;
        }

        public Identity EntityId { get; }
        public string Name { get; }
    }

    public class Event3 : IDomainEvent
    {
        public Event3(Identity entityId, int age)
        {
            EntityId = entityId;
            Age = age;
        }

        public Identity EntityId { get; }
        public int Age { get; }
    }

    public class Event4 : IDomainEvent
    {
        public Event4(Identity entityId, int age)
        {
            EntityId = entityId;
            Age = age;
        }

        public Identity EntityId { get; }
        public int Age { get; }
    }
}