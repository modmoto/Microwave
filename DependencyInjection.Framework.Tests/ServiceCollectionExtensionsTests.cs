using System;
using System.Linq;
using Application.Framework;
using Domain.Framework;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DependencyInjection.Framework.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAllEmptyQuerries()
        {
            var serviceCollection = (IServiceCollection) new ServiceCollection();
            serviceCollection.AddQuerryAndEventHandler(typeof(TestQuery).Assembly);
            var buildServiceProvider = serviceCollection.BuildServiceProvider();
            var querryInDi = (TestQuery) buildServiceProvider.GetService(typeof(TestQuery));
            var allowedEventsOfQuerry =
                (SubscribedEventTypes<TestQuery>) buildServiceProvider.GetService(
                    typeof(SubscribedEventTypes<TestQuery>));
            var querryHandler = buildServiceProvider.GetService<TestQuerryHandler>();
            var querryHandlers = buildServiceProvider.GetServices<IEventHandler>().ToList();

            Assert.Equal(nameof(TestQuerryCreatedEvent), allowedEventsOfQuerry[0].Name);
            Assert.Equal(nameof(TestQuerryNameChangedEvent), allowedEventsOfQuerry[1].Name);
            Assert.Equal(querryInDi, querryHandler.QueryObject);
            Assert.Equal(2, querryHandlers.Count);
        }

        [Fact]
        public void AddAllEventHandlers()
        {
            var serviceCollection = (IServiceCollection) new ServiceCollection();
            serviceCollection.AddQuerryAndEventHandler(typeof(TestReactiveEventHandler).Assembly);
            var buildServiceProvider = serviceCollection.BuildServiceProvider();
            var allowedEventsOfQuerry =
                (SubscribedEventTypes<TestReactiveEventHandler>) buildServiceProvider.GetService(
                    typeof(SubscribedEventTypes<TestReactiveEventHandler>));
            var eventHandler = buildServiceProvider.GetService<TestQuerryHandler>();
            var eventHandlers = buildServiceProvider.GetServices<IEventHandler>().ToList();

            Assert.Equal(nameof(TestQuerryNameChangedEvent), allowedEventsOfQuerry[0].Name);
            Assert.NotNull(eventHandler);
            Assert.Equal(2, eventHandlers.Count);
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

    public class TestQuerryHandler : QueryEventHandler<TestQuery>
    {
        public TestQuerryHandler(TestQuery queryObject, SubscribedEventTypes<TestQuery> subscribedEventTypes) : base(
            queryObject, subscribedEventTypes)
        {
        }
    }

    public class TestReactiveEventHandler : ReactiveEventHandler<TestReactiveEventHandler>
    {
        public TestReactiveEventHandler(SubscribedEventTypes<TestReactiveEventHandler> subscribedEventTypes) : base(
            subscribedEventTypes, null)
        {
        }

        public void Apply(TestQuerryNameChangedEvent domainEvent)
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