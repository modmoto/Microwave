using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microwave.Application;
using Microwave.Domain;
using Microwave.WebApi;
using NUnit.Framework;

namespace Microwave.DependencyInjectionExtensions.UnitTests
{
    [TestFixture]
    public class ExtensionTests
    {
        [Test]
        public void AddDiContainerTest()
        {
            var collection = (IServiceCollection) new ServiceCollection();
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json")
                .Build();

            var storeDependencies = collection.AddMyEventStoreDependencies(typeof(TestEventHandler).Assembly, config);
            var buildServiceProvider = storeDependencies.BuildServiceProvider();

            var delegateHandler1 = buildServiceProvider.GetServices<IHandleAsync<TestDomainEvent>>();
            var delegateHandler2 = buildServiceProvider.GetServices<IHandleAsync<TestDomainEvent2>>();

            var handlers1 = delegateHandler1.ToList();
            var handlers2 = delegateHandler2.ToList();
            Assert.AreEqual(2, handlers1.Count);
            Assert.IsNotNull(handlers1[0] as TestEventHandler);
            Assert.IsNotNull(handlers1[1] as TestEventHandler2);
            Assert.AreEqual(1, handlers2.Count);
            Assert.IsNotNull(handlers2[0] as TestEventHandler);

            var eventDelegateHandler = buildServiceProvider.GetServices<IEventDelegateHandler>().ToList();
            Assert.NotNull(eventDelegateHandler[0] as EventDelegateHandler<TestDomainEvent>);
            Assert.NotNull(eventDelegateHandler[1] as EventDelegateHandler<TestDomainEvent2>);

            var eventFeed1 = buildServiceProvider.GetServices<IEventFeed<TestDomainEvent>>().SingleOrDefault();
            var eventFeed2 = buildServiceProvider.GetServices<IEventFeed<TestDomainEvent2>>().SingleOrDefault();
            Assert.NotNull(eventFeed1 as EventFeed<TestDomainEvent>);
            Assert.NotNull(eventFeed2 as EventFeed<TestDomainEvent2>);

            var qHandler1 = buildServiceProvider.GetServices<IQueryEventHandler>().ToList();
            Assert.NotNull(qHandler1[0] as QueryEventHandler<TestQuery, TestDomainEvent>);
            Assert.NotNull(qHandler1[1] as QueryEventHandler<TestQuery, TestDomainEvent2>);
            Assert.NotNull(qHandler1[2] as QueryEventHandler<TestQuery2, TestDomainEvent>);

            var identHandler = buildServiceProvider.GetServices<IIdentifiableQueryEventHandler>().ToList();
            Assert.NotNull(identHandler[0] as IdentifiableQueryEventHandler<TestIdQuery, TestDomainEvent>);
            Assert.NotNull(identHandler[1] as IdentifiableQueryEventHandler<TestIdQuery, TestDomainEvent2>);
            Assert.NotNull(identHandler[2] as IdentifiableQueryEventHandler<TestIdQuery2, TestDomainEvent>);
        }
    }

    public class TestIdQuery : IdentifiableQuery, IHandle<TestDomainEvent>, IHandle<TestDomainEvent2>
    {
        public Task HandleAsync(TestDomainEvent domainEvent)
        {
            throw new NotImplementedException();
        }

        public Task HandleAsync(TestDomainEvent2 domainEvent)
        {
            throw new NotImplementedException();
        }

        public void Handle(TestDomainEvent domainEvent)
        {
            throw new NotImplementedException();
        }

        public void Handle(TestDomainEvent2 domainEvent)
        {
            throw new NotImplementedException();
        }
    }

    public class TestIdQuery2 : IdentifiableQuery, IHandle<TestDomainEvent>
    {
        public Task HandleAsync(TestDomainEvent domainEvent)
        {
            throw new NotImplementedException();
        }

        public void Handle(TestDomainEvent domainEvent)
        {
            throw new NotImplementedException();
        }
    }

    public class TestQuery : Query, IHandle<TestDomainEvent>, IHandle<TestDomainEvent2>
    {
        public void Handle(TestDomainEvent domainEvent)
        {
            throw new NotImplementedException();
        }

        public void Handle(TestDomainEvent2 domainEvent)
        {
            throw new NotImplementedException();
        }
    }

    public class TestQuery2 : Query, IHandle<TestDomainEvent>
    {
        public void Handle(TestDomainEvent domainEvent)
        {
            throw new NotImplementedException();
        }
    }

    public class TestEventHandler : IHandleAsync<TestDomainEvent>, IHandleAsync<TestDomainEvent2>
    {
        public Task HandleAsync(TestDomainEvent domainEvent)
        {
            throw new NotImplementedException();
        }

        public Task HandleAsync(TestDomainEvent2 domainEvent)
        {
            throw new NotImplementedException();
        }
    }

    public class TestEventHandler2 : IHandleAsync<TestDomainEvent>
    {
        public Task HandleAsync(TestDomainEvent domainEvent)
        {
            throw new NotImplementedException();
        }
    }

    public class TestDomainEvent2 : IDomainEvent
    {
        public TestDomainEvent2(Guid entityId)
        {
            EntityId = entityId;
        }

        public Guid EntityId { get; }
    }

    public class TestDomainEvent : IDomainEvent
    {
        public TestDomainEvent(Guid entityId)
        {
            EntityId = entityId;
        }

        public Guid EntityId { get; }
    }
}