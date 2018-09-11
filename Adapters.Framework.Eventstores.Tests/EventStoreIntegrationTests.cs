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
        public async Task InitializeAsync()
        {
            _eventStoreConnection = EventStoreConnection.Create(new Uri("tcp://admin:changeit@localhost:1113"), "MyTestCon");
            await _eventStoreConnection.ConnectAsync();
            await _eventStoreConnection.DeleteStreamAsync(new TestEventStoreConfig().EventStream, ExpectedVersion.Any, new UserCredentials("admin", "changeit"));
            await _eventStoreConnection.DeleteStreamAsync($"{new TestEventStoreConfig().EventStream}-{nameof(TestEvent)}", ExpectedVersion.Any, new UserCredentials("admin", "changeit"));
            await _eventStoreConnection.DeleteStreamAsync($"{new TestEventStoreConfig().EventStream}-{nameof(TestCreatedEvent)}", ExpectedVersion.Any, new UserCredentials("admin", "changeit"));
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public async Task AppendAsync_List_NoEventsPersisted()
        {
            var entityId = Guid.NewGuid();
            var domainEvents = new List<DomainEvent> { new TestEvent(entityId, "TestSession1"), new TestEvent(entityId, "TestSession2")};

            var eventStore = new EventStoreFacade(new EventSourcingAtributeStrategy(), _eventStoreConnection, new TestEventStoreConfig(), new DomainEventConverter());
            await eventStore.AppendAsync(domainEvents);
            await Task.Delay(1000);

            Assert.Equal(2, (await eventStore.GetEvents(entityId)).Result.Count());
        }

        [Fact]
        public async Task ApplyStrategy()
        {
            var entityId = Guid.NewGuid();
            var domainEvents = new List<DomainEvent> { new TestCreatedEvent(entityId, "OldName"), new TestChangeNameEvent(entityId, "NewName")};

            var eventStore = new EventStoreFacade(new EventSourcingApplyStrategy(), _eventStoreConnection, new TestEventStoreConfig(), new DomainEventConverter());
            await eventStore.AppendAsync(domainEvents);
            await Task.Delay(1000);
            var testEntity = await eventStore.LoadAsync<TestEntity>(entityId);

            Assert.Equal("NewName", testEntity.Result.Name);
            Assert.Equal(entityId, testEntity.Result.Id);
        }

        [Fact]
        public async Task GetEventsMoreThan100()
        {
            var eventStore = new EventStoreFacade(new EventSourcingApplyStrategy(), _eventStoreConnection, new TestEventStoreConfig(), new DomainEventConverter());
            var entityId = Guid.NewGuid();

            var testCreatedEvent = new TestCreatedEvent(entityId, "OldName");

            var domainEvents = new List<DomainEvent>();
            domainEvents.Add(testCreatedEvent);

            for (int i = 0; i <= 120; i++)
            {
                var testChangeNameEvent = new TestChangeNameEvent(entityId, $"NewName{i}");
                domainEvents.Add(testChangeNameEvent);
            }

            await eventStore.AppendAsync(domainEvents);
            await Task.Delay(3000);
            var testEntity = await eventStore.LoadAsync<TestEntity>(entityId);

            Assert.Equal("NewName120", testEntity.Result.Name);
            Assert.Equal(entityId, testEntity.Result.Id);
        }

        [Fact]
        public async Task Append_VersionConflict()
        {
            var eventStore = new EventStoreFacade(new EventSourcingApplyStrategy(), _eventStoreConnection, new TestEventStoreConfig(), new DomainEventConverter());
            var entityId = Guid.NewGuid();

            var domainEvents = new List<DomainEvent> { new TestCreatedEvent(entityId, "OldName"), new TestChangeNameEvent(entityId, "NewName")};

            await eventStore.AppendAsync(domainEvents);

            await Task.Delay(1000);
            var testEntity = await eventStore.LoadAsync<TestEntity>(entityId);

            var domainEventsNew = new List<DomainEvent> { new TestChangeNameEvent(entityId, "NewName2")};
            var convertedElements = domainEventsNew.Select(eve => new EventData(Guid.NewGuid(), eve.GetType().Name, true,
                Encoding.UTF8.GetBytes(new DomainEventConverter().Serialize(eve)), null));;
            await _eventStoreConnection.AppendToStreamAsync(new TestEventStoreConfig().EventStream, ExpectedVersion.Any,
                convertedElements);

            await Task.Delay(1000);

            var domainEventsCreateInBetween = new List<DomainEvent> { new TestChangeNameEvent(entityId, "NewName3")};
            Assert.Equal("NewName", testEntity.Result.Name);
            Assert.Equal(entityId, testEntity.Result.Id);
            await Assert.ThrowsAsync<WrongExpectedVersionException>(async () => await eventStore.AppendAsync(domainEventsCreateInBetween, testEntity.EntityVersion));
        }

        [Fact]
        public async Task Append_NoVersionConflict()
        {
            var eventStore = new EventStoreFacade(new EventSourcingApplyStrategy(), _eventStoreConnection, new TestEventStoreConfig(), new DomainEventConverter());
            var entityId = Guid.NewGuid();

            var domainEvents = new List<DomainEvent> { new TestCreatedEvent(entityId, "OldName"), new TestChangeNameEvent(entityId, "NewName")};

            await eventStore.AppendAsync(domainEvents);

            await Task.Delay(1000);
            var testEntity = await eventStore.LoadAsync<TestEntity>(entityId);

            var domainEventsCreateInBetween = new List<DomainEvent> { new TestChangeNameEvent(entityId, "NewName2")};

            await eventStore.AppendAsync(domainEventsCreateInBetween, testEntity.EntityVersion);
            await Task.Delay(1000);

            var testEntityAfterBetweenCommig = await eventStore.LoadAsync<TestEntity>(entityId);

            Assert.Equal("NewName", testEntity.Result.Name);
            Assert.Equal("NewName2", testEntityAfterBetweenCommig.Result.Name);
            Assert.Equal(entityId, testEntity.Result.Id);
            Assert.Equal(entityId, testEntityAfterBetweenCommig.Result.Id);
        }

//        [Fact]
//        public async Task Append_NoVersionConflictOnDifferentEntity()
//        {
//            var eventStore = new EventStoreFacade(new EventSourcingApplyStrategy(), _eventStoreConnection, new TestEventStoreConfig(), new DomainEventConverter());
//            var entityId = Guid.NewGuid();
//
//            var domainEvents = new List<DomainEvent> { new TestCreatedEvent(entityId, "OldName"), new TestChangeNameEvent(entityId, "NewName")};
//
//            await eventStore.AppendAsync(domainEvents);
//
//            await Task.Delay(1000);
//            var testEntity = await eventStore.LoadAsync<TestEntity>(entityId);
//
//            var newGuid = Guid.NewGuid();
//            var domainEventsNew = new List<DomainEvent> { new TestCreatedEvent(newGuid, "MyDifferentThing")};
//            var convertedElements = domainEventsNew.Select(eve => new EventData(Guid.NewGuid(), eve.GetType().Name, true,
//                Encoding.UTF8.GetBytes(new DomainEventConverter().Serialize(eve)), null));;
//            await _eventStoreConnection.AppendToStreamAsync(new TestEventStoreConfig().EventStream, ExpectedVersion.Any,
//                convertedElements);
//
//            await Task.Delay(1000);
//
//            var domainEventsCreateInBetween = new List<DomainEvent> { new TestChangeNameEvent(entityId, "NewName3")};
//            Assert.Equal("NewName", testEntity.Result.Name);
//            Assert.Equal(entityId, testEntity.Result.Id);
//            await eventStore.AppendAsync(domainEventsCreateInBetween, testEntity.EntityVersion);
//
//            var testEntityNew = await eventStore.LoadAsync<TestEntity>(newGuid);
//            Assert.Equal("MyDifferentThing", testEntityNew.Result.Name);
//            Assert.Equal(newGuid, testEntityNew.Result.Id);
//
//            var testEntityUpdated = await eventStore.LoadAsync<TestEntity>(entityId);
//            Assert.Equal("NewName3", testEntityUpdated.Result.Name);
//            Assert.Equal(entityId, testEntityUpdated.Result.Id);
//        }
    }

    internal class TestChangeNameEvent : DomainEvent
    {
        public string NewName { get; }

        public TestChangeNameEvent(Guid entityId, string newName) : base(entityId)
        {
            NewName = newName;
        }
    }

    internal class TestCreatedEvent : DomainEvent
    {
        public string Name { get; }

        public TestCreatedEvent(Guid entityId, string name) : base(entityId)
        {
            Name = name;
        }
    }

    internal class TestEntity : Entity
    {
        public void Apply(TestCreatedEvent domainEvent)
        {
            Id = domainEvent.EntityId;
            Name = domainEvent.Name;
        }

        public void Apply(TestChangeNameEvent domainEvent)
        {
            Name = domainEvent.NewName;
        }

        public string Name { get; private set; }
    }

    internal class TestEvent : DomainEvent
    {
        public string Name { get; }

        public TestEvent(Guid guid, string name) : base(guid)
        {
            Name = name;
        }
    }
}