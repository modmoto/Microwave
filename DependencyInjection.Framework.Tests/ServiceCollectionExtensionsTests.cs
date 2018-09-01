using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Adapters.Framework.EventStores;
using Adapters.Json.ObjectPersistences;
using Application.Framework;
using Domain.Framework;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DependencyInjection.Framework.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public async Task AddAllLoadedQuerries()
        {
            var eventStore = new EventStore(new DomainEventPersister(), new EventSourcingApplyStrategy());
            var entityId = Guid.NewGuid();
            var domainEvents = new List<DomainEvent>
            {
                new TestQuerryCreatedEvent(entityId, "NameFirst"),
                new TestQuerryNameChangedEvent(entityId, "NameSecond")
            };

            await eventStore.AppendAsync(domainEvents);

            var serviceCollection = (IServiceCollection) new ServiceCollection();
            serviceCollection.AddAllLoadedQuerries(typeof(TestQuerry).Assembly);
            var buildServiceProvider = serviceCollection.BuildServiceProvider();
            var querryInDi = (TestQuerry) buildServiceProvider.GetService(typeof(TestQuerry));

            Assert.Equal("NameSecond", querryInDi.Name);
        }
    }

    public class TestQuerry : Querry
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