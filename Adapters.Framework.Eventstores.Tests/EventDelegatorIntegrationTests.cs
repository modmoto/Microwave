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
            _eventStoreConnection =
                EventStoreConnection.Create(new Uri("tcp://admin:changeit@localhost:1113"), "MyTestCon");
            await _eventStoreConnection.ConnectAsync();
            await _eventStoreConnection.DeleteStreamAsync($"{new TestEventStoreConfig().ReadStream}-TestEv1",
                ExpectedVersion.Any, new UserCredentials("admin", "changeit"));
            await _eventStoreConnection.DeleteStreamAsync($"{new TestEventStoreConfig().ReadStream}-TestEv2",
                ExpectedVersion.Any, new UserCredentials("admin", "changeit"));
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
                new TestEv1(entityGuid) {Name = "Name1"},
                new TestEv2(entityGuid) {Name = "Name2", LastName = "LastName2"},
                new TestEv1(entityGuid) {Name = "Name3"}
            };
            var eventStoreFacade = new EventStoreFacade(new EventSourcingApplyStrategy(), _eventStoreConnection,
                new TestEventStoreConfig(), new DomainEventConverter());
            var eventStoreSub = new EventStoreSubscribtion(_eventStoreConnection, new TestEventStoreConfig(),
                new DomainEventConverter());

            var queryObject1 = new TestQuery1();
            var queryObject2 = new TestQuery2();
            var testQueryHandler1 = new TestQueryEventHandler1(new TestQuery1(), new SubscribedEventTypes<TestQuery1>());
            var testQueryHandler2 = new TestQueryEventHandler2(new TestQuery2(), new SubscribedEventTypes<TestQuery2>());

            var queryEventDelegator = new QueryEventDelegator(
                new List<IQuerryEventHandler>
                {
                    testQueryHandler1,
                    testQueryHandler2
                }, new HandlerVersionRepository(_eventStoreConnection, new TestEventStoreConfig()),
                eventStoreSub, new List<IReactiveEventHandler>());

            queryEventDelegator.SubscribeToStreams();

            await Task.Delay(5000);

            await eventStoreFacade.AppendAsync(domainEvents, -1);

            await Task.Delay(5000);

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
                new TestEv1(entityGuid) {Name = "Name1"},
                new TestEv2(entityGuid) {Name = "Name2", LastName = "LastName2"},
                new TestEv1(entityGuid) {Name = "Name3"}
            };
            var eventStoreFacade = new EventStoreFacade(new EventSourcingApplyStrategy(), _eventStoreConnection,
                new TestEventStoreConfig(), new DomainEventConverter());
            var eventStoreSub = new EventStoreSubscribtion(_eventStoreConnection, new TestEventStoreConfig(),
                new DomainEventConverter());

            var queryObject1 = new TestQuery1();
            var queryObject2 = new TestQuery2();
            var testQueryHandler1 = new TestQueryEventHandler1(queryObject1, new SubscribedEventTypes<TestQuery1>());
            var testQueryHandler2 = new TestQueryEventHandler2(queryObject2, new SubscribedEventTypes<TestQuery2>());

            await eventStoreFacade.AppendAsync(domainEvents, -1);

            await Task.Delay(1000);

            var queryEventDelegator = new QueryEventDelegator(
                new List<IQuerryEventHandler>
                {
                    testQueryHandler1,
                    testQueryHandler2
                }, new HandlerVersionRepository(_eventStoreConnection, new TestEventStoreConfig()),
                eventStoreSub, new List<IReactiveEventHandler>());

            queryEventDelegator.SubscribeToStreams();

            await Task.Delay(10000);

            Assert.Equal("Name3", queryObject1.Name);
            Assert.Equal(2, queryObject1.Count);
            Assert.Equal("LastName2", queryObject1.LastName);
            Assert.Equal("Name2", queryObject2.Name);
        }
    }

    internal class TestQueryEventHandler1 : QueryEventHandler<TestQuery1>
    {
        public TestQueryEventHandler1(TestQuery1 queryObject, SubscribedEventTypes<TestQuery1> subscribedEventTypes) : base(
            queryObject, subscribedEventTypes)
        {
        }
    }

    internal class TestQueryEventHandler2 : QueryEventHandler<TestQuery2>
    {
        public TestQueryEventHandler2(TestQuery2 queryObject, SubscribedEventTypes<TestQuery2> subscribedEventTypes) : base(
            queryObject, subscribedEventTypes)
        {
        }
    }

    internal class TestQuery1 : Query
    {
        public string Name { get; set; }

        public string LastName { get; set; }

        public int Count { get; set; }

        public void Apply(TestEv1 testEv1)
        {
            Name = testEv1.Name;
            Count++;
        }

        public void Apply(TestEv2 testEvent1)
        {
            LastName = testEvent1.LastName;
        }
    }

    internal class TestEv2 : DomainEvent
    {
        public TestEv2(Guid entityId) : base(entityId)
        {
        }

        public string LastName { get; set; }
        public string Name { get; set; }
    }

    internal class TestEv1 : DomainEvent
    {
        public TestEv1(Guid entityId) : base(entityId)
        {
        }

        public string Name { get; set; }
    }

    internal class TestQuery2 : Query
    {
        public string Name { get; set; }

        public void Apply(TestEv2 testEvent1)
        {
            Name = testEvent1.Name;
        }
    }
}