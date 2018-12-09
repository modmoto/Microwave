using System;
using System.Linq;
using System.Reflection;
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


            var eventFeeds = buildServiceProvider.GetServices<IEventFeed>().ToList();

            var eventFeed1 = eventFeeds[0];
            var eventFeed2 = eventFeeds[1];
            Assert.IsTrue(eventFeed1 is EventTypeFeed<TestDomainEvent>);
            Assert.IsTrue(eventFeed2 is EventTypeFeed<TestDomainEvent2>);

            var eventOverallClients = buildServiceProvider.GetServices<IOverallEventFeed>().ToList();
            Assert.IsTrue(eventOverallClients[0] is ReadModelFeed<TestReadModel>);
            Assert.IsTrue(eventOverallClients[1] is ReadModelFeed<TestIdQuery>);
            Assert.IsTrue(eventOverallClients[2] is ReadModelFeed<TestIdQuerySingle>);
            Assert.IsTrue(eventOverallClients[3] is ReadModelFeed<TestIdQuery2>);

            var type = eventOverallClients[0].GetType();
            var fieldInfo = type.GetField("_domainEventClient", BindingFlags.NonPublic | BindingFlags.Instance);
            var value = (DomainOverallEventClient<TestReadModel>) fieldInfo.GetValue(eventOverallClients[0]);
            Assert.AreEqual("http://localhost:5000/Api/DomainEvents", value.BaseAddress.ToString());

            var qHandler1 = buildServiceProvider.GetServices<IQueryEventHandler>().ToList();
            Assert.IsTrue(qHandler1[0] is QueryEventHandler<TestQuery, TestDomainEvent>);
            Assert.IsTrue(qHandler1[1] is QueryEventHandler<TestQuery, TestDomainEvent2>);
            Assert.IsTrue(qHandler1[2] is QueryEventHandler<TestQuery2, TestDomainEvent>);

            var identHandler = buildServiceProvider.GetServices<IReadModelHandler>().ToList();
            Assert.IsTrue(identHandler[0] is ReadModelHandler<TestReadModel>);
            Assert.IsTrue(identHandler[1] is ReadModelHandler<TestIdQuery>);
            Assert.IsTrue(identHandler[2] is ReadModelHandler<TestIdQuerySingle>);
            Assert.IsTrue(identHandler[3] is ReadModelHandler<TestIdQuery2>);
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

            var eventFeed1 = buildServiceProvider.GetServices<IEventFeed>().FirstOrDefault();
            var identHandler = buildServiceProvider.GetServices<IReadModelHandler>().ToList();
            Assert.IsTrue(identHandler[0] is ReadModelHandler<TestReadModel>);
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

    public class TestIdQuery : ReadModel
    {
        public void Handle(TestDomainEvent domainEvent)
        {
        }

        public void Handle(TestDomainEvent2 domainEvent)
        {
        }
    }

    public class TestIdQuerySingle : ReadModel
    {
        public void Handle(TestDomainEvent3 domainEvent)
        {
        }
    }

    public class TestDomainEvent3 : IDomainEvent
    {
        public Guid EntityId { get; }
    }

    public class TestIdQuery2 : ReadModel
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