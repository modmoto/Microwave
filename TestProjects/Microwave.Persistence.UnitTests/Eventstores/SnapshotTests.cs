using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain.EventSourcing;
using Microwave.EventStores;
using Microwave.EventStores.SnapShots;
using Microwave.Persistence.UnitTestsSetup;

namespace Microwave.Persistence.UnitTests.Eventstores
{
    [TestClass]
    public class SnapshotTests
    {
        [TestMethod]
        [PersistenceTypeTest]
        public async Task SnapshotGetSavedAfterThirdEvent(PersistenceLayerProvider layerProvider)
        {
            var snapShotRepository = layerProvider.SnapShotRepository;
            var eventStore = new EventStore(layerProvider.EventRepository, snapShotRepository
                , new SnapShotConfig(new
            List<ISnapShot> { new SnapShot<User>(3)}));

            var entityId = Guid.NewGuid();
            await eventStore.AppendAsync(new List<IDomainEvent>
            {
                new Event1(entityId),
                new Event2(entityId, "Peter")
            }, 0);

            await eventStore.LoadAsync<User>(entityId.ToString());

            var snapShotDboOld = await snapShotRepository.LoadSnapShot<User>(entityId.ToString());
            Assert.IsNull(snapShotDboOld.Value.Id);
            Assert.IsNull(snapShotDboOld.Value.Name);

            await eventStore.AppendAsync(new List<IDomainEvent>
            {
                new Event3(entityId, 14),
                new Event2(entityId, "PeterNeu")
            }, 2);

            var eventstoreResult = await eventStore.LoadAsync<User>(entityId.ToString());

            var user = eventstoreResult;
            Assert.AreEqual(4, eventstoreResult.Version);
            Assert.AreEqual(14, user.Value.Age);
            Assert.AreEqual("PeterNeu", user.Value.Name);
            Assert.AreEqual(entityId, user.Value.Id);

            var snapShotDbo = await snapShotRepository.LoadSnapShot<User>(entityId.ToString());

            Assert.AreEqual(4, snapShotDbo.Version);
            Assert.AreEqual(entityId, snapShotDbo.Value.Id);
            var userSnapShot = snapShotDbo.Value;

            Assert.AreEqual(14, userSnapShot.Age);
            Assert.AreEqual("PeterNeu", userSnapShot.Name);
            Assert.AreEqual(entityId, userSnapShot.Id);
        }

        [TestMethod]
        [PersistenceTypeTest]
        public async Task SnapshotExactlyOnSnapShotTime_DoesNotReturnNotFoundBug(PersistenceLayerProvider layerProvider)
        {
            var eventStore = new EventStore(layerProvider.EventRepository, layerProvider.SnapShotRepository);

            var entityId = Guid.NewGuid();
            await eventStore.AppendAsync(new List<IDomainEvent>
            {
                new Event1(entityId),
                new Event2(entityId, "Peter"),
                new Event2(entityId, "Peterneu")
            }, 0);

            await eventStore.LoadAsync<User>(entityId.ToString());
            var result = await eventStore.LoadAsync<User>(entityId.ToString());

            Assert.AreEqual("Peterneu", result.Value.Name);
            Assert.AreEqual(3, result.Version);
        }
    }

    public class User : Entity, IApply<Event1>, IApply<Event2>, IApply<Event3>
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string Id { get; set; }

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
        public Event1(Guid entityId)
        {
            EntityId = entityId.ToString();
        }

        public string EntityId { get; }
    }

    public class Event2 : IDomainEvent
    {
        public Event2(Guid entityId, string name)
        {
            EntityId = entityId.ToString();
            Name = name;
        }

        public string EntityId { get; }
        public string Name { get; }
    }

    public class Event3 : IDomainEvent
    {
        public Event3(Guid entityId, int age)
        {
            EntityId = entityId.ToString();
            Age = age;
        }

        public string EntityId { get; }
        public int Age { get; }
    }

    public class Event4 : IDomainEvent
    {
        public Event4(string entityId, int age)
        {
            EntityId = entityId;
            Age = age;
        }

        public string EntityId { get; }
        public int Age { get; }
    }
}