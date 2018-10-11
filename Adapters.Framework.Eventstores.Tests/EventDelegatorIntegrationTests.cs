using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Adapters.Framework.EventStores;
using Adapters.Json.ObjectPersistences;
using Application.Framework;
using Domain.Framework;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Xunit;

namespace Adapters.Framework.Eventstores.Tests
{
    public class EventDelegatorIntegrationTests : IAsyncLifetime
    {
        private IEventStoreConnection _eventStoreConnection;
        public async Task InitializeAsync()
        {
            _eventStoreConnection = EventStoreConnection.Create(new Uri("tcp://admin:changeit@localhost:1113"), "MyTestCon");
            await _eventStoreConnection.ConnectAsync();
            await _eventStoreConnection.DeleteStreamAsync($"{new TestEventStoreConfig().ReadStream}-TestEvent1", ExpectedVersion.Any, new UserCredentials("admin", "changeit"));
            await _eventStoreConnection.DeleteStreamAsync($"{new TestEventStoreConfig().ReadStream}-TestEvent2", ExpectedVersion.Any, new UserCredentials("admin", "changeit"));
            await Task.Delay(1000);
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
                new TestEvent1(entityGuid) {Name = "Name3"}
            };
            var eventStoreFacade = new EventStoreFacade(new EventSourcingApplyStrategy(), _eventStoreConnection, new TestEventStoreConfig(), new DomainEventConverter());
            var eventStoreSub = new EventStoreSubscribtion(_eventStoreConnection, new TestEventStoreConfig(), new DomainEventConverter());

            var testQueryHandler1 = new TestQueryEventHandler1(new TestQ1(), new SubscribedEventTypes<TestQ1>());
            var testQueryHandler2 = new TestQueryEventHandler2(new TestQ2(), new SubscribedEventTypes<TestQ2>());

            var queryEventDelegator = new QueryEventDelegator(
                new List<IEventHandler>
                {
                    testQueryHandler1,
                    testQueryHandler2
                }, eventStoreFacade, new HandlerVersionRepository(_eventStoreConnection, new TestEventStoreConfig()), eventStoreSub);

            queryEventDelegator.SubscribeToStreams();

            await Task.Delay(5000);

            await eventStoreFacade.AppendAsync(domainEvents, -1);

            await Task.Delay(5000);

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
                new TestEvent1(entityGuid) {Name = "Name3"}
            };
            var eventStoreFacade = new EventStoreFacade(new EventSourcingApplyStrategy(), _eventStoreConnection, new TestEventStoreConfig(), new DomainEventConverter());
            var eventStoreSub = new EventStoreSubscribtion(_eventStoreConnection, new TestEventStoreConfig(), new DomainEventConverter());

            var testQueryHandler1 = new TestQueryEventHandler1(new TestQ1(), new SubscribedEventTypes<TestQ1>());
            var testQueryHandler2 = new TestQueryEventHandler2(new TestQ2(), new SubscribedEventTypes<TestQ2>());

            await eventStoreFacade.AppendAsync(domainEvents, -1);

            await Task.Delay(1000);

            var queryEventDelegator = new QueryEventDelegator(
                new List<IEventHandler>
                {
                    testQueryHandler1,
                    testQueryHandler2
                }, eventStoreFacade, new HandlerVersionRepository(_eventStoreConnection, new TestEventStoreConfig()), eventStoreSub);

            queryEventDelegator.SubscribeToStreams();

            await Task.Delay(5000);

            var queryObject1 = testQueryHandler1.QueryObject;
            var queryObject2 = testQueryHandler2.QueryObject;

            Assert.Equal("Name3", queryObject1.Name);
            Assert.Equal("LastName2", queryObject1.LastName);
            Assert.Equal("Name2", queryObject2.Name);
        }
    }

    internal class TestQueryEventHandler1 : QueryEventHandler<TestQ1>
    {
        public TestQueryEventHandler1(TestQ1 queryObject, SubscribedEventTypes<TestQ1> subscribedEventTypes) : base(
            queryObject, subscribedEventTypes)
        {
        }
    }

    internal class TestQueryEventHandler2 : QueryEventHandler<TestQ2>
    {
        public TestQueryEventHandler2(TestQ2 queryObject, SubscribedEventTypes<TestQ2> subscribedEventTypes) : base(
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