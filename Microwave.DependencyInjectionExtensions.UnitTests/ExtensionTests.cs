using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain;
using Microwave.EventStores;
using Microwave.Queries;
using Microwave.WebApi;

namespace Microwave.DependencyInjectionExtensions.UnitTests
{
    [TestClass]
    public class ExtensionTests
    {
        [TestMethod]
        public void AddDiContainerTest()
        {
            var collection = (IServiceCollection) new ServiceCollection();
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json")
                .Build();

            var storeDependencies = collection.AddMicrowaveQuerries(typeof(TestEventHandler).Assembly, config);
            var buildServiceProvider = storeDependencies.BuildServiceProvider();

            var delegateHandler1 = buildServiceProvider.GetServices<IHandleAsync<TestDomainEvent>>();
            var delegateHandler2 = buildServiceProvider.GetServices<IHandleAsync<TestDomainEvent2>>();

            var handlers1 = delegateHandler1.ToList();
            var handlers2 = delegateHandler2.ToList();
            Assert.AreEqual(2, handlers1.Count);
            Assert.IsTrue(handlers1[0] is TestEventHandler);
            Assert.IsTrue(handlers1[1] is TestEventHandler2);
            Assert.AreEqual(1, handlers2.Count);
            Assert.IsTrue(handlers2[0] is TestEventHandler);

            var eventDelegateHandler = buildServiceProvider.GetServices<IEventDelegateHandler>().ToList();
            Assert.IsTrue(eventDelegateHandler[0] is EventDelegateHandler<TestDomainEvent>);
            Assert.IsTrue(eventDelegateHandler[1] is EventDelegateHandler<TestDomainEvent2>);

            var eventFeed1 = buildServiceProvider.GetServices<IEventFeed<TestDomainEvent>>().SingleOrDefault();
            var eventFeed2 = buildServiceProvider.GetServices<IEventFeed<TestDomainEvent2>>().SingleOrDefault();
            var eventFeed3 = buildServiceProvider.GetServices<IEventFeed<TestDomainEvent3>>().SingleOrDefault();
            Assert.IsTrue(eventFeed1 is EventTypeFeed<TestDomainEvent>);
            Assert.IsTrue(eventFeed2 is EventTypeFeed<TestDomainEvent2>);
            Assert.IsTrue(eventFeed3 is EventTypeFeed<TestDomainEvent3>);

            var qHandler1 = buildServiceProvider.GetServices<IQueryEventHandler>().ToList();
            Assert.IsTrue(qHandler1[0] is QueryEventHandler<TestQuery, TestDomainEvent>);
            Assert.IsTrue(qHandler1[1] is QueryEventHandler<TestQuery, TestDomainEvent2>);
            Assert.IsTrue(qHandler1[2] is QueryEventHandler<TestQuery2, TestDomainEvent>);

            var identHandler = buildServiceProvider.GetServices<IReadModelHandler>().ToList();
            Assert.IsTrue(identHandler[0] is ReadModelHandler<TestIdQuery, TestDomainEvent>);
            Assert.IsTrue(identHandler[1] is ReadModelHandler<TestIdQuery, TestDomainEvent2>);
            Assert.IsTrue(identHandler[2] is ReadModelHandler<TestIdQuerySingle, TestDomainEvent3>);
            Assert.IsTrue(identHandler[3] is ReadModelHandler<TestIdQuery2, TestDomainEvent>);
        }

        [TestMethod]
        public void AddDiContainerTest_Twice()
        {
            var collection = (IServiceCollection) new ServiceCollection();
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json")
                .Build();

            var storeDependencies = collection
                .AddMicrowaveQuerries(typeof(TestEventHandler).Assembly, config)
                .AddMicrowaveQuerries(typeof(TestEventHandler).Assembly, config);

            var buildServiceProvider = storeDependencies.BuildServiceProvider();

            var eventFeed1 = buildServiceProvider.GetServices<IEventFeed<TestDomainEvent>>().FirstOrDefault();
            var identHandler = buildServiceProvider.GetServices<IReadModelHandler>().ToList();
            Assert.IsTrue(identHandler[0] is ReadModelHandler<TestIdQuery, TestDomainEvent>);
            Assert.IsTrue(eventFeed1 is EventTypeFeed<TestDomainEvent>);
        }

        [TestMethod]
        public void AddMicrowaveDependencies()
        {
            var collection = (IServiceCollection) new ServiceCollection();

            var storeDependencies = collection.AddMicrowave();

            var buildServiceProvider = storeDependencies.BuildServiceProvider();

            var store = buildServiceProvider.GetServices<IEventStore>().FirstOrDefault();
            Assert.IsNotNull(store);
        }
    }

    public class TestIdQuery : ReadModel, IHandle<TestDomainEvent>, IHandle<TestDomainEvent2>
    {
        public void Handle(TestDomainEvent domainEvent)
        {
        }

        public void Handle(TestDomainEvent2 domainEvent)
        {
        }
    }

    public class TestIdQuerySingle : ReadModel, IHandle<TestDomainEvent3>
    {
        public void Handle(TestDomainEvent3 domainEvent)
        {
        }
    }

    public class TestDomainEvent3 : IDomainEvent
    {
        public Guid EntityId { get; }
    }

    public class TestIdQuery2 : ReadModel, IHandle<TestDomainEvent>
    {
        public void Handle(TestDomainEvent domainEvent)
        {
        }
    }

    public class TestQuery : Query, IHandle<TestDomainEvent>, IHandle<TestDomainEvent2>
    {
        public void Handle(TestDomainEvent domainEvent)
        {
        }

        public void Handle(TestDomainEvent2 domainEvent)
        {
        }
    }

    public class TestQuery2 : Query, IHandle<TestDomainEvent>
    {
        public void Handle(TestDomainEvent domainEvent)
        {
        }
    }

    public class TestEventHandler : IHandleAsync<TestDomainEvent>, IHandleAsync<TestDomainEvent2>
    {
        public Task HandleAsync(TestDomainEvent domainEvent)
        {
            return Task.CompletedTask;
        }

        public Task HandleAsync(TestDomainEvent2 domainEvent)
        {
            return Task.CompletedTask;
        }
    }

    public class TestEventHandler2 : IHandleAsync<TestDomainEvent>
    {
        public Task HandleAsync(TestDomainEvent domainEvent)
        {
            return  Task.CompletedTask;
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