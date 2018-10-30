using System;
using System.Linq;
using System.Threading.Tasks;
using Adapters.Framework.WebApi;
using Application.Framework;
using Domain.Framework;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Microsoft.Extensions.Configuration;

namespace DependencyInjection.Framework.UnitTests
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

            var delegateHandlers = buildServiceProvider.GetServices<IEventDelegateHandler>().ToList();
            Assert.AreEqual(2, delegateHandlers.Count);
            Assert.NotNull(delegateHandlers[0] as EventDelegateHandler<TestDomainEvent>);
            Assert.NotNull(delegateHandlers[1] as EventDelegateHandler<TestDomainEvent2>);

            var eventFeed1 = buildServiceProvider.GetServices<IEventFeed<TestDomainEvent>>().Single();
            var eventFeed2 = buildServiceProvider.GetServices<IEventFeed<TestDomainEvent2>>().Single();
            Assert.NotNull(eventFeed1 as EventFeed<TestDomainEvent>);
            Assert.NotNull(eventFeed2 as EventFeed<TestDomainEvent2>);

            var qHandler1 = buildServiceProvider.GetServices<IQueryEventHandler>().ToList();
            Assert.NotNull(qHandler1[0] as QueryEventHandler<TestQuery, TestDomainEvent>);
            Assert.NotNull(qHandler1[1] as QueryEventHandler<TestQuery, TestDomainEvent2>);
            Assert.NotNull(qHandler1[2] as QueryEventHandler<TestQuery2, TestDomainEvent>);
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

    public class TestDomainEvent2 : DomainEvent
    {
        public TestDomainEvent2(Guid entityId) : base(entityId)
        {
        }
    }

    public class TestDomainEvent : DomainEvent
    {
        public TestDomainEvent(Guid entityId) : base(entityId)
        {
        }
    }
}