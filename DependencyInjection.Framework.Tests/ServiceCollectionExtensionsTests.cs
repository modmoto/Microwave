using System;
using System.Linq;
using System.Threading.Tasks;
using Application.Framework;
using Domain.Framework;
using EventStore.ClientAPI;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DependencyInjection.Framework.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAllReactiveEventHandlers()
        {
            var serviceCollection = (IServiceCollection) new ServiceCollection();

            serviceCollection.AddMyEventStoreDependencies(typeof(TestReactiveEventHandler).Assembly);
            var buildServiceProvider = serviceCollection.BuildServiceProvider();

            var nameChangedHandler = buildServiceProvider.GetServices(typeof(IHandleAsync<TestQuerryNameChangedEvent>));
            var createdHandler = buildServiceProvider.GetServices(typeof(IHandleAsync<TestQuerryCreatedEvent>));

            var eventHandlerName = buildServiceProvider.GetService<HandlerDelegator<TestQuerryNameChangedEvent>>();
            var eventHandlerCreated = buildServiceProvider.GetService<HandlerDelegator<TestQuerryNameChangedEvent>>();

            Assert.Equal(2, nameChangedHandler.Count());
            Assert.Equal(1, createdHandler.Count());
            Assert.NotNull(eventHandlerName);
            Assert.NotNull(eventHandlerCreated);
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

    public class TestHandler : QueryEventHandler<TestQuery>
    {
        public TestHandler(TestQuery queryObject, SubscribedEventTypes<TestQuery> subscribedEventTypes) : base(
            queryObject, subscribedEventTypes)
        {
        }
    }

    public class TestReactiveEventHandler : IHandleAsync<TestQuerryNameChangedEvent>
    {
        public Task HandleAsync(TestQuerryNameChangedEvent domainEvent)
        {
            return Task.CompletedTask;
        }
    }

    public class TestReactiveEventHandler2 : IHandleAsync<TestQuerryNameChangedEvent>, IHandleAsync<TestQuerryCreatedEvent>
    {
        public Task HandleAsync(TestQuerryNameChangedEvent domainEvent)
        {
            return Task.CompletedTask;
        }

        public Task HandleAsync(TestQuerryCreatedEvent domainEvent)
        {
            return Task.CompletedTask;
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