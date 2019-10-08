using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain.EventSourcing;
using Microwave.Domain.Identities;
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

            var entityId = GuidIdentity.Create(Guid.NewGuid());
            await eventStore.AppendAsync(new List<IDomainEvent>
            {
                new Event1(entityId),
                new Event2(entityId, "Peter")
            }, 0);

            await eventStore.LoadAsync<User>(entityId);

            var snapShotDboOld = await snapShotRepository.LoadSnapShot<User>(entityId);
            Assert.IsNull(snapShotDboOld.Value.Id);
            Assert.IsNull(snapShotDboOld.Value.Name);

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

            var snapShotDbo = await snapShotRepository.LoadSnapShot<User>(entityId);

            Assert.AreEqual(4, snapShotDbo.Version);
            Assert.AreEqual(entityId.Id, snapShotDbo.Value.Id.Id);
            var userSnapShot = snapShotDbo.Value;

            Assert.AreEqual(14, userSnapShot.Age);
            Assert.AreEqual("PeterNeu", userSnapShot.Name);
            Assert.AreEqual(entityId.Id, userSnapShot.Id.Id);
        }

        [TestMethod]
        [PersistenceTypeTest]
        public async Task SnapshotExactlyOnSnapShotTime_DoesNotReturnNotFoundBug(PersistenceLayerProvider layerProvider)
        {
            var eventStore = new EventStore(layerProvider.EventRepository, layerProvider.SnapShotRepository);

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