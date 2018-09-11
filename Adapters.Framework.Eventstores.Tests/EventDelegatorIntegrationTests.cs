using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adapters.Framework.EventStores;
using Adapters.Json.ObjectPersistences;
using Application.Framework;
using Domain.Framework;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Xunit;


[assembly: CollectionBehavior(MaxParallelThreads = 1)]
namespace Adapters.Framework.Eventstores.Tests
{
    public class EventDelegatorIntegrationTests : IAsyncLifetime
    {
        private IEventStoreConnection _eventStoreConnection;
        public async Task InitializeAsync()
        {
            _eventStoreConnection = EventStoreConnection.Create(new Uri("tcp://admin:changeit@localhost:1113"), "MyTestCon");
            await _eventStoreConnection.ConnectAsync();
            await _eventStoreConnection.DeleteStreamAsync(new TestEventStoreConfig().EventStream, ExpectedVersion.Any, new UserCredentials("admin", "changeit"));
            await _eventStoreConnection.DeleteStreamAsync($"{new TestEventStoreConfig().EventStream}-{nameof(TestEvent1)}", ExpectedVersion.Any, new UserCredentials("admin", "changeit"));
            await _eventStoreConnection.DeleteStreamAsync($"{new TestEventStoreConfig().EventStream}-{nameof(TestEvent2)}", ExpectedVersion.Any, new UserCredentials("admin", "changeit"));
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public async Task ActivateAndListenToSubscriptions()
        {
            var entityGuid = Guid.NewGuid();
            var domainEvents = new List<DomainEvent>
            {
                new TestEvent1(entityGuid) {Name = "Name1"},
                new TestEvent2(entityGuid) {Name = "Name2", LastName = "LastName2"},
                new TestEvent1(entityGuid) {Name = "Name3"},
            };
            var eventStoreFacade = new EventStoreFacade(new EventSourcingApplyStrategy(), _eventStoreConnection, new TestEventStoreConfig(), new DomainEventConverter());

            var testQueryHandler1 = new TestQueryHandler1(new TestQ1(), new SubscribedEventTypes<TestQ1>());
            var testQueryHandler2 = new TestQueryHandler2(new TestQ2(), new SubscribedEventTypes<TestQ2>());
            var queryEventDelegator = new QueryEventDelegator(
                new List<IQueryHandler>
                {
                    testQueryHandler1,
                    testQueryHandler2
                }, eventStoreFacade);

            queryEventDelegator.SubscribeToStreamsFrom();

            var convertedElements = domainEvents.Select(eve => new EventData(Guid.NewGuid(), eve.GetType().Name, true,
                Encoding.UTF8.GetBytes(new DomainEventConverter().Serialize(eve)), null));

            await _eventStoreConnection.AppendToStreamAsync(new TestEventStoreConfig().EventStream, ExpectedVersion.Any,
                convertedElements);

            await Task.Delay(2000);

            var queryObject1 = testQueryHandler1.QueryObject;
            var queryObject2 = testQueryHandler2.QueryObject;

            Assert.Equal("Name3", queryObject1.Name);
            Assert.Equal("LastName2", queryObject1.LastName);
            Assert.Equal("Name2", queryObject2.Name);
        }

        [Fact]
        public async Task ActivateAndListenToSubscriptions_AfterEventsAreAdded()
        {
            var entityGuid = Guid.NewGuid();
            var domainEvents = new List<DomainEvent>
            {
                new TestEvent1(entityGuid) {Name = "Name1"},
                new TestEvent2(entityGuid) {Name = "Name2", LastName = "LastName2"},
                new TestEvent1(entityGuid) {Name = "Name3"},
            };
            var eventStoreFacade = new EventStoreFacade(new EventSourcingApplyStrategy(), _eventStoreConnection, new TestEventStoreConfig(), new DomainEventConverter());

            var testQueryHandler1 = new TestQueryHandler1(new TestQ1(), new SubscribedEventTypes<TestQ1>());
            var testQueryHandler2 = new TestQueryHandler2(new TestQ2(), new SubscribedEventTypes<TestQ2>());

            var convertedElements = domainEvents.Select(eve => new EventData(Guid.NewGuid(), eve.GetType().Name, true,
                Encoding.UTF8.GetBytes(new DomainEventConverter().Serialize(eve)), null));

            await _eventStoreConnection.AppendToStreamAsync(new TestEventStoreConfig().EventStream, ExpectedVersion.Any,
                convertedElements);

            await Task.Delay(2000);

            var queryEventDelegator = new QueryEventDelegator(
                new List<IQueryHandler>
                {
                    testQueryHandler1,
                    testQueryHandler2
                }, eventStoreFacade);

            queryEventDelegator.SubscribeToStreamsFrom();

            await Task.Delay(1000);

            var queryObject1 = testQueryHandler1.QueryObject;
            var queryObject2 = testQueryHandler2.QueryObject;

            Assert.Equal("Name3", queryObject1.Name);
            Assert.Equal("LastName2", queryObject1.LastName);
            Assert.Equal("Name2", queryObject2.Name);
        }

        [Fact]
        public async Task ActivateAndRecallSubscriptions()
        {
            var entityGuid = Guid.NewGuid();
            var domainEvents = new List<DomainEvent>
            {
                new TestEvent1(entityGuid) {Name = "Name1"},
                new TestEvent2(entityGuid) {Name = "Name2", LastName = "LastName2"},
                new TestEvent1(entityGuid) {Name = "Name3"},
            };
            var eventStoreFacade = new EventStoreFacade(new EventSourcingApplyStrategy(), _eventStoreConnection, new TestEventStoreConfig(), new DomainEventConverter());

            var testQueryHandler1 = new TestQueryHandler1(new TestQ1(), new SubscribedEventTypes<TestQ1>());
            var testQueryHandler2 = new TestQueryHandler2(new TestQ2(), new SubscribedEventTypes<TestQ2>());
            var queryEventDelegator = new QueryEventDelegator(
                new List<IQueryHandler>
                {
                    testQueryHandler1,
                    testQueryHandler2
                }, eventStoreFacade);


            await queryEventDelegator.SubscribeToStreams();

            var convertedElements = domainEvents.Select(eve => new EventData(Guid.NewGuid(), eve.GetType().Name, true,
                Encoding.UTF8.GetBytes(new DomainEventConverter().Serialize(eve)), null));

            await _eventStoreConnection.AppendToStreamAsync(new TestEventStoreConfig().EventStream, ExpectedVersion.Any,
                convertedElements);

            await Task.Delay(2000);

            var queryObject1 = testQueryHandler1.QueryObject;
            var queryObject2 = testQueryHandler2.QueryObject;

            Assert.Equal("Name3", queryObject1.Name);
            Assert.Equal("LastName2", queryObject1.LastName);
            Assert.Equal("Name2", queryObject2.Name);
        }
    }

    internal class TestQueryHandler1 : QueryHandler<TestQ1>
    {
        public TestQueryHandler1(TestQ1 queryObject, SubscribedEventTypes<TestQ1> subscribedEventTypes) : base(
            queryObject, subscribedEventTypes)
        {
        }
    }

    internal class TestQueryHandler2 : QueryHandler<TestQ2>
    {
        public TestQueryHandler2(TestQ2 queryObject, SubscribedEventTypes<TestQ2> subscribedEventTypes) : base(
            queryObject, subscribedEventTypes)
        {
        }
    }

    internal class TestQ1 : Query
    {
        public string Name { get; set; }

        public string LastName { get; set; }

        public void Apply(TestEvent1 testEvent1)
        {
            Name = testEvent1.Name;
            Count++;
        }

        public int Count { get; set; }

        public void Apply(TestEvent2 testEvent1)
        {
            LastName = testEvent1.LastName;
        }
    }

    internal class TestEvent2 : DomainEvent
    {
        public string LastName { get; set; }
        public string Name { get; set; }

        public TestEvent2(Guid entityId) : base(entityId)
        {
        }
    }

    internal class TestEvent1 : DomainEvent
    {
        public string Name { get; set; }

        public TestEvent1(Guid entityId) : base(entityId)
        {
        }
    }

    internal class TestQ2 : Query
    {
        public string Name { get; set; }

        public void Apply(TestEvent2 testEvent1)
        {
            Name = testEvent1.Name;
        }
    }
}