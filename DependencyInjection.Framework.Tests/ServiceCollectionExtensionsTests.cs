using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adapters.Framework.EventStores;
using Adapters.Json.ObjectPersistences;
using Application.Framework;
using Domain.Framework;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DependencyInjection.Framework.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public async Task AddAllLoadedQuerries()
        {
            var eventStoreConnection = EventStoreConnection.Create(new Uri("tcp://admin:changeit@localhost:1113"), "MyTestCon");
            await eventStoreConnection.ConnectAsync();
            await eventStoreConnection.DeleteStreamAsync(new TestEventStoreConfig().EventStream, ExpectedVersion.Any,
                new UserCredentials("admin", "changeit"));
            var eventStore = new EventStoreFacade(new EventSourcingApplyStrategy(), eventStoreConnection, new TestEventStoreConfig(), new DomainEventConverter());
            var entityId = Guid.NewGuid();
            var domainEvents = new List<DomainEvent>
            {
                new TestQuerryCreatedEvent(entityId, "NameFirst"),
                new TestQuerryNameChangedEvent(entityId, "NameSecond")
            };

            await eventStore.AppendAsync(domainEvents);

            var serviceCollection = (IServiceCollection) new ServiceCollection();
            serviceCollection.AddAllLoadedQuerries(typeof(TestQuery).Assembly, eventStore);
            var buildServiceProvider = serviceCollection.BuildServiceProvider();
            var querryInDi = (TestQuery) buildServiceProvider.GetService(typeof(TestQuery));
            var allowedEventsOfQuerry = (SubscribedEventTypes<TestQuery>) buildServiceProvider.GetService(typeof(SubscribedEventTypes<TestQuery>));
            var querryHandler = buildServiceProvider.GetService<TestQuerryHandler>();
            var querryHandlers = buildServiceProvider.GetServices<IQueryHandler>().ToList();

            Assert.Equal("NameSecond", querryInDi.Name);
            Assert.Equal(nameof(TestQuerryCreatedEvent), allowedEventsOfQuerry[0].Name);
            Assert.Equal(nameof(TestQuerryNameChangedEvent), allowedEventsOfQuerry[1].Name);
            Assert.Equal(querryInDi, querryHandler.QueryObject);
            Assert.Equal(1, querryHandlers.Count);
        }
    }

    public class TestQuery : Query
    {
        public string Name { get; private set; }

        public void Apply(TestQuerryCreatedEvent domainEvent)
        {
            Name = domainEvent.Name;
        }

        public void Apply(TestQuerryNameChangedEvent domainEvent)
        {
            Name = domainEvent.Name;
        }
    }

    public class TestQuerryHandler : QueryHandler<TestQuery>
    {
        public TestQuerryHandler(TestQuery queryObject, SubscribedEventTypes<TestQuery> subscribedEventTypes) : base(queryObject, subscribedEventTypes)
        {
        }
    }

    public class TestQuerryNameChangedEvent : DomainEvent
    {
        public TestQuerryNameChangedEvent(Guid entityId, string name) : base(entityId)
        {
            Name = name;
        }

        public string Name { get; }
    }

    public class TestQuerryCreatedEvent : DomainEvent
    {
        public TestQuerryCreatedEvent(Guid entityId, string name) : base(entityId)
        {
            Name = name;
        }

        public string Name { get; }
    }
}