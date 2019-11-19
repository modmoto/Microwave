using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain.EventSourcing;
using Microwave.EventStores;
using Microwave.EventStores.SnapShots;
using Microwave.Persistence.MongoDb.Eventstores;
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
            BsonMapRegistrationHelpers.AddBsonMapFor<Event1>();
            BsonMapRegistrationHelpers.AddBsonMapFor<Event2>();
            BsonMapRegistrationHelpers.AddBsonMapFor<Event3>();

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
            Assert.AreEqual(entityId.ToString(), user.Value.Id);

            var snapShotDbo = await snapShotRepository.LoadSnapShot<User>(entityId.ToString());

            Assert.AreEqual(4, snapShotDbo.Version);
            Assert.AreEqual(entityId.ToString(), snapShotDbo.Value.Id);
            var userSnapShot = snapShotDbo.Value;

            Assert.AreEqual(14, userSnapShot.Age);
            Assert.AreEqual("PeterNeu", userSnapShot.Name);
            Assert.AreEqual(entityId.ToString(), userSnapShot.Id);
        }

        [TestMethod]
        [PersistenceTypeTest]
        public async Task SnapshotExactlyOnSnapShotTime_DoesNotReturnNotFoundBug(PersistenceLayerProvider layerProvider)
        {
            BsonMapRegistrationHelpers.AddBsonMapFor<Event1>();
            BsonMapRegistrationHelpers.AddBsonMapFor<Event2>();

            var eventStore = new EventStore(layerProvider.EventRepository, layerProvider.SnapShotRepository, new
            SnapShotConfig(new List<ISnapShot> { new SnapShot<User>(1) }));

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

        [TestMethod]
        [PersistenceTypeTest]
        public async Task SnapshotIsChangedAfterSavingIt_InMemoryBug(PersistenceLayerProvider layerProvider)
        {
            BsonMapRegistrationHelpers.AddBsonMapFor<Event1>();
            BsonMapRegistrationHelpers.AddBsonMapFor<Event2>();

            var eventStore = new EventStore(layerProvider.EventRepository, layerProvider.SnapShotRepository, new
            SnapShotConfig(new List<ISnapShot> { new SnapShot<UserWithAutoAply>(2)}));

            var event1 = UserWithAutoAply.Create();
            var userWithAutoAply = new UserWithAutoAply();
            userWithAutoAply.Apply(event1);
            var changeNameEvent1 = userWithAutoAply.AppendName("neuer Name");
            var changeNameEvent2 = userWithAutoAply.AppendName("neuer Name");
            var changeNameEvent3 = userWithAutoAply.AppendName("neuer Name");

            var entityId = event1.Id;
            await eventStore.AppendAsync(new List<IDomainEvent>
            {
                event1, changeNameEvent1, changeNameEvent2, changeNameEvent3
            }, 0);

            var result = await eventStore.LoadAsync<UserWithAutoAply>(entityId.ToString());

            var userLoaded = result.Value;
            Assert.AreEqual(3, userLoaded.Names.Count());
            Assert.AreEqual(4, result.Version);

            userLoaded.AppendName("new stuff");

            Assert.AreEqual(4, userLoaded.Names.Count());

            var resultLoadedAgain = await eventStore.LoadAsync<UserWithAutoAply>(entityId.ToString());
            Assert.AreEqual(3, resultLoadedAgain.Value.Names.Count());
        }
    }
    public class UserWithAutoAply : Entity, IApply<Event2>, IApply<Event1>
    {
        public static Event1 Create()
        {
            return new Event1(Guid.NewGuid());
        }

        public Event2 AppendName(string newName)
        {
            var event2 = new Event2(Id, newName);
            Apply(event2);
            return event2;
        }

        public Guid Id { get; private set; }

        public IEnumerable<string> Names { get; private set; } = new List<string>();

        public void Apply(Event1 domainEvent)
        {
            Id = domainEvent.Id;
        }

        public void Apply(Event2 domainEvent)
        {
            var list = Names.ToList();
            list.Add(domainEvent.Name);
            Names = list;
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
        public Event1(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }

        public string EntityId => Id.ToString();
    }

    public class Event2 : IDomainEvent
    {
        public Event2(Guid id, string name)
        {
            Name = name;
            Id = id;
        }

        public Guid Id { get; }

        public string EntityId => Id.ToString();
        public string Name { get; }
    }

    public class Event3 : IDomainEvent
    {
        public Event3(Guid id, int age)
        {
            Age = age;
            Id = id;
        }

        public Guid Id { get; }

        public string EntityId => Id.ToString();
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