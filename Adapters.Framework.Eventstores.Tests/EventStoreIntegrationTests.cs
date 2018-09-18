using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adapters.Framework.EventStores;
using Adapters.Json.ObjectPersistences;
using Domain.Framework;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using EventStore.ClientAPI.SystemData;
using Xunit;

namespace Adapters.Framework.Eventstores.Tests
{
    public class EventStoreIntegrationTests : IAsyncLifetime
    {
        private IEventStoreConnection _eventStoreConnection;
        private Guid _entityId;

        public async Task InitializeAsync()
        {
            _entityId = Guid.NewGuid();
            _eventStoreConnection =
                EventStoreConnection.Create(new Uri("tcp://admin:changeit@localhost:1113"), "MyTestCon");
            await _eventStoreConnection.ConnectAsync();

        }

        public async Task DisposeAsync()
        {
            await _eventStoreConnection.DeleteStreamAsync($"{new TestEventStoreConfig().WriteStream}-{_entityId}", ExpectedVersion.Any,
                new UserCredentials("admin", "changeit"));
        }

        [Fact]
        public async Task Append_NoVersionConflict()
        {
            var eventStore = new EventStoreFacade(new EventSourcingApplyStrategy(), _eventStoreConnection,
                new TestEventStoreConfig(), new DomainEventConverter());

            var domainEvents = new List<DomainEvent>
                {new TestCreatedEvent(_entityId, "OldName"), new TestChangeNameEvent(_entityId, "NewName")};

            await eventStore.AppendAsync(domainEvents, 0);

            var testEntity = await eventStore.LoadAsync<TestEntity>(_entityId);

            var domainEventsCreateInBetween = new List<DomainEvent> {new TestChangeNameEvent(_entityId, "NewName2")};

            await eventStore.AppendAsync(domainEventsCreateInBetween, testEntity.EntityVersion);

            var testEntityAfterBetweenCommig = await eventStore.LoadAsync<TestEntity>(_entityId);

            Assert.Equal(1, testEntity.EntityVersion);
            Assert.Equal("NewName", testEntity.Result.Name);
            Assert.Equal("NewName2", testEntityAfterBetweenCommig.Result.Name);
            Assert.Equal(_entityId, testEntity.Result.Id);
            Assert.Equal(_entityId, testEntityAfterBetweenCommig.Result.Id);
        }

        [Fact]
        public async Task Append_VersionConflict()
        {
            var eventStore = new EventStoreFacade(new EventSourcingApplyStrategy(), _eventStoreConnection,
                new TestEventStoreConfig(), new DomainEventConverter());

            var domainEvents = new List<DomainEvent>
                {new TestCreatedEvent(_entityId, "OldName"), new TestChangeNameEvent(_entityId, "NewName")};

            await eventStore.AppendAsync(domainEvents, 0);

            var testEntity = await eventStore.LoadAsync<TestEntity>(_entityId);

            var domainEventsNew = new List<DomainEvent> {new TestChangeNameEvent(_entityId, "NewName2")};
            var convertedElements = domainEventsNew.Select(eve => new EventData(Guid.NewGuid(), eve.GetType().Name,
                true,
                Encoding.UTF8.GetBytes(new DomainEventConverter().Serialize(eve)), null));
            ;
            await _eventStoreConnection.AppendToStreamAsync($"{new TestEventStoreConfig().WriteStream}-{_entityId}", ExpectedVersion.Any,
                convertedElements);

            var domainEventsCreateInBetween = new List<DomainEvent> {new TestChangeNameEvent(_entityId, "NewName3")};
            Assert.Equal("NewName", testEntity.Result.Name);
            Assert.Equal(_entityId, testEntity.Result.Id);
            await Assert.ThrowsAsync<WrongExpectedVersionException>(async () =>
                await eventStore.AppendAsync(domainEventsCreateInBetween, testEntity.EntityVersion));
        }

        [Fact]
        public async Task AppendAsync_List_NoEventsPersisted()
        {
            var domainEvents = new List<DomainEvent>
                {new TestEvent(_entityId, "TestSession1"), new TestEvent(_entityId, "TestSession2")};

            var eventStore = new EventStoreFacade(new EventSourcingAtributeStrategy(), _eventStoreConnection,
                new TestEventStoreConfig(), new DomainEventConverter());
            await eventStore.AppendAsync(domainEvents, 0);

            Assert.Equal(2, (await eventStore.GetEvents(_entityId)).Result.Count());
        }

        [Fact]
        public async Task ApplyStrategy()
        {
            var domainEvents = new List<DomainEvent>
                {new TestCreatedEvent(_entityId, "OldName"), new TestChangeNameEvent(_entityId, "NewName")};

            var eventStore = new EventStoreFacade(new EventSourcingApplyStrategy(), _eventStoreConnection,
                new TestEventStoreConfig(), new DomainEventConverter());
            await eventStore.AppendAsync(domainEvents, 0);
            var testEntity = await eventStore.LoadAsync<TestEntity>(_entityId);

            Assert.Equal("NewName", testEntity.Result.Name);
            Assert.Equal(_entityId, testEntity.Result.Id);
        }

        [Fact]
        public async Task GetEventsMoreThan100()
        {
            var eventStore = new EventStoreFacade(new EventSourcingApplyStrategy(), _eventStoreConnection,
                new TestEventStoreConfig(), new DomainEventConverter());

            var testCreatedEvent = new TestCreatedEvent(_entityId, "OldName");

            var domainEvents = new List<DomainEvent>();
            domainEvents.Add(testCreatedEvent);

            for (var i = 0; i <= 120; i++)
            {
                var testChangeNameEvent = new TestChangeNameEvent(_entityId, $"NewName{i}");
                domainEvents.Add(testChangeNameEvent);
            }

            await eventStore.AppendAsync(domainEvents, 0);
            var testEntity = await eventStore.LoadAsync<TestEntity>(_entityId);

            Assert.Equal("NewName120", testEntity.Result.Name);
            Assert.Equal(_entityId, testEntity.Result.Id);
        }

        [Fact]
        public async Task Append_NoVersionConflictOnDifferentEntity()
        {
            var eventStore = new EventStoreFacade(new EventSourcingApplyStrategy(), _eventStoreConnection, new TestEventStoreConfig(), new DomainEventConverter());

            var domainEvents = new List<DomainEvent> { new TestCreatedEvent(_entityId, "OldName"), new TestChangeNameEvent(_entityId, "NewName")};

            await eventStore.AppendAsync(domainEvents, 0);

            var testEntity = await eventStore.LoadAsync<TestEntity>(_entityId);

            var newGuid = Guid.NewGuid();
            var domainEventsNew = new List<DomainEvent> { new TestCreatedEvent(newGuid, "MyDifferentThing")};
            var convertedElements = domainEventsNew.Select(eve => new EventData(Guid.NewGuid(), eve.GetType().Name, true,
                Encoding.UTF8.GetBytes(new DomainEventConverter().Serialize(eve)), null));
            await _eventStoreConnection.AppendToStreamAsync($"{new TestEventStoreConfig().WriteStream}-{newGuid}", ExpectedVersion.Any,
                convertedElements);

            var domainEventsCreateInBetween = new List<DomainEvent> { new TestChangeNameEvent(_entityId, "NewName3")};
            Assert.Equal("NewName", testEntity.Result.Name);
            Assert.Equal(_entityId, testEntity.Result.Id);
            await eventStore.AppendAsync(domainEventsCreateInBetween, testEntity.EntityVersion);

            var testEntityNew = await eventStore.LoadAsync<TestEntity>(newGuid);
            Assert.Equal("MyDifferentThing", testEntityNew.Result.Name);
            Assert.Equal(newGuid, testEntityNew.Result.Id);

            var testEntityUpdated = await eventStore.LoadAsync<TestEntity>(_entityId);
            Assert.Equal("NewName3", testEntityUpdated.Result.Name);
            Assert.Equal(_entityId, testEntityUpdated.Result.Id);
        }

        [Fact]
        public async Task Load_NestingEntity()
        {
            var eventStore = new EventStoreFacade(new EventSourcingAtributeStrategy(), _eventStoreConnection,
                new TestEventStoreConfig(), new DomainEventConverter());
            var childId = Guid.NewGuid();

            var domainEvents = new List<DomainEvent>
            {
                new TestCreatedNestedEntityEvent(_entityId, "ParentName"),
                new TestCreateNestedChildEntityEvent(childId, "OldChildName"),
                new TestAddedNestedChildEntityToParent(_entityId, childId),
                new TestChangeNestedChildEntityNameEvent(childId, "NewChildName")
            };

            await eventStore.AppendAsync(domainEvents, 0);

            var testEntity = await eventStore
                .Include(nameof(TestEntityNestedParent.Child))
                .LoadAsync<TestEntityNestedParent>(_entityId);
            var testEntityChild = await eventStore.LoadAsync<TestEntityNestedChild>(childId);

            Assert.Equal("ParentName", testEntity.Result.ParentName);
            Assert.Equal(_entityId, testEntity.Result.Id);
            Assert.Equal("NewChildName", testEntityChild.Result.ChildName);
            Assert.Equal(childId, testEntityChild.Result.Id);
            Assert.Equal(childId, testEntity.Result.Child.Id);

            Assert.Equal("NewChildName", testEntity.Result.Child.ChildName);
        }

        [Fact]
        public async Task Load_DoubleNestingEntity()
        {
            var eventStore = new EventStoreFacade(new EventSourcingAtributeStrategy(), _eventStoreConnection,
                new TestEventStoreConfig(), new DomainEventConverter());
            var childId = Guid.NewGuid();
            var childChildId = Guid.NewGuid();

            var domainEvents = new List<DomainEvent>
            {
                new TestCreatedNestedEntityEvent(_entityId, "ParentName"),
                new TestCreateNestedChildEntityEvent(childId, "OldChildName"),
                new TestCreateNestedChildEntityEvent(childChildId, "OldNestedNestedChildName"),
                new TestAddNextChildEvent(childId, childChildId),
                new TestAddedNestedChildEntityToParent(_entityId, childId),
                new TestChangeNestedChildEntityNameEvent(childChildId, "NewNestedNestedChildName")
            };

            await eventStore.AppendAsync(domainEvents, 0);

            var testEntity = await eventStore
                .Include("Child")
                .FurtherInclude("Child", "NextChild")
                .LoadAsync<TestEntityNestedParent>(_entityId);
            var testEntityChild = await eventStore.LoadAsync<TestEntityNestedChild>(childChildId);

            Assert.Equal("ParentName", testEntity.Result.ParentName);
            Assert.Equal(_entityId, testEntity.Result.Id);
            Assert.Equal("NewNestedNestedChildName", testEntityChild.Result.ChildName);
            Assert.Equal(childChildId, testEntityChild.Result.Id);
            Assert.Equal(childId, testEntity.Result.Child.Id);
            Assert.Equal(childChildId, testEntity.Result.Child.NextChild.Id);
            Assert.Equal("NewNestedNestedChildName", testEntity.Result.Child.NextChild.ChildName);
        }
    }

    internal class TestChangeNameEvent : DomainEvent
    {
        public TestChangeNameEvent(Guid entityId, string newName) : base(entityId)
        {
            NewName = newName;
        }

        public string NewName { get; }
    }

    internal class TestCreatedEvent : DomainEvent
    {
        public TestCreatedEvent(Guid entityId, string name) : base(entityId)
        {
            Name = name;
        }

        public string Name { get; }
    }

    internal class TestEntity : Entity
    {
        public string Name { get; private set; }

        public void Apply(TestCreatedEvent domainEvent)
        {
            Id = domainEvent.EntityId;
            Name = domainEvent.Name;
        }

        public void Apply(TestChangeNameEvent domainEvent)
        {
            Name = domainEvent.NewName;
        }
    }

    internal class TestCreatedNestedEntityEvent : DomainEvent
    {
        public TestCreatedNestedEntityEvent(Guid entityId, string name) : base(entityId)
        {
            Name = name;
        }

        [ActualPropertyName(nameof(TestEntityNestedParent.ParentName))]
        public string Name { get; }
    }

    internal class TestEntityNestedParent : Entity
    {
        public TestEntityNestedChild Child { get; set; }

        public string ParentName { get; private set; }
    }

    internal class TestAddedNestedChildEntityToParent : DomainEvent
    {
        public TestAddedNestedChildEntityToParent(Guid entityId, Guid childId) : base(entityId)
        {
            ChildId = childId;
        }

        [ActualPropertyName("Child.Id")]
        public Guid ChildId { get; }
    }

    internal class TestCreateNestedChildEntityEvent : DomainEvent
    {
        public TestCreateNestedChildEntityEvent(Guid entityId, string childName) : base(entityId)
        {
            ChildName = childName;
        }

        public string ChildName { get; }
    }

    internal class TestAddNextChildEvent : DomainEvent
    {
        [ActualPropertyName("NextChild.Id")]
        public Guid ChildId { get; }

        public TestAddNextChildEvent(Guid entityId, Guid childId) : base(entityId)
        {
            ChildId = childId;
        }
    }

    internal class TestEntityNestedChild : Entity
    {
        public string ChildName { get; private set; }
        public TestEntityNestedChild NextChild { get; private set; }
    }

    internal class TestChangeNestedChildEntityNameEvent : DomainEvent
    {
        public TestChangeNestedChildEntityNameEvent(Guid entityId, string newName) : base(entityId)
        {
            NewName = newName;
        }

        [ActualPropertyName(nameof(TestEntityNestedChild.ChildName))]

        public string NewName { get; }
    }

    internal class TestEvent : DomainEvent
    {
        public TestEvent(Guid guid, string name) : base(guid)
        {
            Name = name;
        }

        public string Name { get; }
    }
}