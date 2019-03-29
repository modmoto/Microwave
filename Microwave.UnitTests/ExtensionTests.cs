using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Application;
using Microwave.Application.Discovery;
using Microwave.Domain;
using Microwave.EventStores;
using Microwave.EventStores.Ports;
using Microwave.Queries;
using Microwave.UnitTests.PublishedEventsDll;
using Microwave.WebApi;
using MongoDB.Bson.Serialization;

namespace Microwave.DependencyInjectionExtensions.UnitTests
{
    [TestClass]
    public class ExtensionTests
    {
        [TestMethod]
        public void AddDiContainerTest()
        {
            var collection = (IServiceCollection) new ServiceCollection();

            var storeDependencies = collection.AddMicrowave(typeof
            (TestEventHandler).Assembly);
            var buildServiceProvider = storeDependencies.BuildServiceProvider();

            var eventLocation = buildServiceProvider.GetService<IEventLocation>();
            eventLocation.SetDomainEventLocation(new SubscriberEventAndReadmodelConfig(
                new Uri("http://localhost:5002/"),
                new []{ nameof (TestDomainEvent1), nameof(TestDomainEvent2), nameof(TestDomainEvent3) },
                new List<ReadModelSubscription>
                {
                    new ReadModelSubscription(nameof(TestReadModel), new TestReadModel().GetsCreatedOn.Name),
                    new ReadModelSubscription(nameof(TestIdQuery), new TestIdQuery().GetsCreatedOn.Name),
                    new ReadModelSubscription(nameof(TestIdQuery2), new TestIdQuery2().GetsCreatedOn.Name),
                    new ReadModelSubscription(nameof(TestIdQuerySingle), new TestIdQuerySingle().GetsCreatedOn.Name),
                }));


            var eventDelegateHandlers = buildServiceProvider.GetServices<IAsyncEventHandler>().ToList();
            Assert.AreEqual(2, eventDelegateHandlers.Count);
            Assert.IsNotNull(eventDelegateHandlers[0]);
            Assert.IsNotNull(eventDelegateHandlers[1]);

            var handleAsync1 = buildServiceProvider.GetServices<IHandleAsync<TestDomainEvent1>>();
            var handleAsync2 = buildServiceProvider.GetServices<IHandleAsync<TestDomainEvent2>>();

            var handlers1 = handleAsync1.ToList();
            var handlers2 = handleAsync2.ToList();
            Assert.AreEqual(2, handlers1.Count);
            Assert.IsTrue(handlers1[0] is TestEventHandler);
            Assert.IsTrue(handlers1[1] is TestEventHandler2);
            Assert.AreEqual(1, handlers2.Count);
            Assert.IsTrue(handlers2[0] is TestEventHandler);

            var queryFeed1 = buildServiceProvider.GetServices<IEventFeed<QueryEventHandler<TestQuery1, TestDomainEvent1>>>().FirstOrDefault();
            var queryFeed2 = buildServiceProvider.GetServices<IEventFeed<QueryEventHandler<TestQuery1, TestDomainEvent2>>>().FirstOrDefault();
            var queryFeed3 = buildServiceProvider.GetServices<IEventFeed<QueryEventHandler<TestQuery2, TestDomainEvent1>>>().FirstOrDefault();
            Assert.IsNotNull(queryFeed1);
            Assert.IsNotNull(queryFeed2);
            Assert.IsNotNull(queryFeed3);

            var eventFeeds = buildServiceProvider.GetServices<IEventFeed<AsyncEventHandler<TestDomainEvent1>>>().FirstOrDefault();
            var eventFeeds2 = buildServiceProvider.GetServices<IEventFeed<AsyncEventHandler<TestDomainEvent2>>>().FirstOrDefault();
            Assert.IsNotNull(eventFeeds);
            Assert.IsNotNull(eventFeeds2);

            var eventOverallClients1 = buildServiceProvider.GetServices<IEventFeed<ReadModelHandler<TestReadModel>>>().FirstOrDefault();
            var eventOverallClients2 = buildServiceProvider.GetServices<IEventFeed<ReadModelHandler<TestIdQuery>>>().FirstOrDefault();
            var eventOverallClients3 = buildServiceProvider.GetServices<IEventFeed<ReadModelHandler<TestIdQuerySingle>>>().FirstOrDefault();
            var eventOverallClients4 = buildServiceProvider.GetServices<IEventFeed<ReadModelHandler<TestIdQuery2>>>().FirstOrDefault();
            Assert.IsTrue(eventOverallClients1 is EventFeed<ReadModelHandler<TestReadModel>>);
            Assert.IsTrue(eventOverallClients2 is EventFeed<ReadModelHandler<TestIdQuery>>);
            Assert.IsTrue(eventOverallClients3 is EventFeed<ReadModelHandler<TestIdQuerySingle>>);
            Assert.IsTrue(eventOverallClients4 is EventFeed<ReadModelHandler<TestIdQuery2>>);

            var type = eventOverallClients1.GetType();
            var fieldInfo = type.GetField("_domainEventClient", BindingFlags.NonPublic | BindingFlags.Instance);
            var value = (DomainEventClient<ReadModelHandler<TestReadModel>>) fieldInfo.GetValue(eventOverallClients1);
            Assert.AreEqual("http://localhost:5002/Api/DomainEvents", value.BaseAddress.ToString());

            var typeQueryFeed = queryFeed1.GetType();
            var fieldInfoQueryFeed = typeQueryFeed.GetField("_domainEventClient", BindingFlags.NonPublic | BindingFlags.Instance);
            var valueQueryFeed = (DomainEventClient<QueryEventHandler<TestQuery1, TestDomainEvent1>>) fieldInfoQueryFeed.GetValue(queryFeed1);
            Assert.AreEqual("http://localhost:5002/Api/DomainEventTypeStreams/TestDomainEvent1", valueQueryFeed.BaseAddress.ToString());


            var qHandler1 = buildServiceProvider.GetServices<IQueryEventHandler>().ToList();
            Assert.AreEqual(3, qHandler1.Count);
            Assert.IsTrue(qHandler1[0] is QueryEventHandler<TestQuery1, TestDomainEvent1>);
            Assert.IsTrue(qHandler1[1] is QueryEventHandler<TestQuery1, TestDomainEvent2>);
            Assert.IsTrue(qHandler1[2] is QueryEventHandler<TestQuery2, TestDomainEvent1>);

            var identHandler = buildServiceProvider.GetServices<IReadModelHandler>().ToList();
            Assert.AreEqual(4, identHandler.Count);
            Assert.IsTrue(identHandler[0] is ReadModelHandler<TestReadModel>);
            Assert.IsTrue(identHandler[1] is ReadModelHandler<TestIdQuery>);
            Assert.IsTrue(identHandler[2] is ReadModelHandler<TestIdQuerySingle>);
            Assert.IsTrue(identHandler[3] is ReadModelHandler<TestIdQuery2>);

            var eventRegister = buildServiceProvider.GetServices<EventRegistration>().Single();
            Assert.AreEqual(eventRegister[nameof(TestDomainEvent1)], typeof(TestDomainEvent1));
            Assert.AreEqual(eventRegister[nameof(TestDomainEvent2)], typeof(TestDomainEvent2));
            Assert.AreEqual(eventRegister[nameof(TestDomainEvent3)], typeof(TestDomainEvent3));
        }

        [TestMethod]
        public void AddDiContainerTest_Twice()
        {
            var collection = (IServiceCollection) new ServiceCollection();

            var storeDependencies = collection
                .AddMicrowave(typeof(TestEventHandler).Assembly)
                .AddMicrowave(typeof(TestEventHandler).Assembly);

            var buildServiceProvider = storeDependencies.BuildServiceProvider();

            var eventLocation = buildServiceProvider.GetService<IEventLocation>();
            eventLocation.SetDomainEventLocation(new SubscriberEventAndReadmodelConfig(
                new Uri("http://some-uri.de"),
                new []{ nameof (TestDomainEvent1) },
                new List<ReadModelSubscription>
                {
                    new ReadModelSubscription(nameof(TestReadModel), new TestReadModel().GetsCreatedOn.Name),
                    new ReadModelSubscription(nameof(TestIdQuery), new TestIdQuery().GetsCreatedOn.Name),
                    new ReadModelSubscription(nameof(TestIdQuery2), new TestIdQuery2().GetsCreatedOn.Name),
                    new ReadModelSubscription(nameof(TestIdQuerySingle), new TestIdQuerySingle().GetsCreatedOn.Name),
                }));

            var eventFeed1 = buildServiceProvider.GetServices<IEventFeed<AsyncEventHandler<TestDomainEvent1>>>().FirstOrDefault();
            Assert.IsNotNull(eventFeed1);
            var identHandler = buildServiceProvider.GetServices<IReadModelHandler>().ToList();
            Assert.IsTrue(identHandler[0] is ReadModelHandler<TestReadModel>);
        }

        [TestMethod]
        public void AddMicrowaveDependencies()
        {
            var collection = (IServiceCollection) new ServiceCollection();

            var storeDependencies = collection.AddMicrowave(typeof(TestDomainEvent1).Assembly);

            var buildServiceProvider = storeDependencies.BuildServiceProvider();

            var store = buildServiceProvider.GetServices<IEventStore>().FirstOrDefault();
            Assert.IsNotNull(store);

            Assert.IsTrue(BsonClassMap.IsClassMapRegistered(typeof(TestDomainEvent1)));
            Assert.IsTrue(BsonClassMap.IsClassMapRegistered(typeof(TestDomainEvent2)));
            Assert.IsTrue(BsonClassMap.IsClassMapRegistered(typeof(TestDomainEvent3)));
        }

        [TestMethod]
        public void AddMicrowaveDependencies_PublishedEventsCorrect()
        {
            var collection = (IServiceCollection) new ServiceCollection();

            var storeDependencies = collection.AddMicrowave(typeof(TestDomainEvent_PublishedEvent1).Assembly);

            var buildServiceProvider = storeDependencies.BuildServiceProvider();

            var publishingEventRegistration = buildServiceProvider.GetServices<PublishedEventCollection>().Single().ToList();
            Assert.AreEqual(nameof(TestDomainEvent_PublishedEvent1), publishingEventRegistration[0]);
            Assert.AreEqual(nameof(TestDomainEvent_PublishedEvent3), publishingEventRegistration[1]);
            Assert.AreEqual(2, publishingEventRegistration.Count);

            var discoveryController = buildServiceProvider.GetServices<DiscoveryController>().Single();
            Assert.IsNotNull(discoveryController);
        }

        [TestMethod]
        public void AddMicrowaveDependencies_SubscribedEventsCorrect()
        {
            var collection = (IServiceCollection) new ServiceCollection();
            var storeDependencies = collection.AddMicrowave(typeof(TestHandle).Assembly);

            var buildServiceProvider = storeDependencies.BuildServiceProvider();

            var publishingEventRegistration = buildServiceProvider.GetServices<SubscribedEventCollection>().Single();
            var ihandleAsyncEvents = publishingEventRegistration.IHandleAsyncEvents.ToList();
            Assert.AreEqual(nameof(TestDomainEvent_PublishedEvent1), ihandleAsyncEvents[0]);
            Assert.AreEqual(nameof(TestDomainEvent_PublishedEvent2), ihandleAsyncEvents[1]);
            Assert.AreEqual(2, ihandleAsyncEvents.Count);

            var discoveryController = buildServiceProvider.GetServices<DiscoveryController>().Single();
            Assert.IsNotNull(discoveryController);
        }

        [TestMethod]
        public void AddMicrowaveDependencies_ReadModelsCorrect()
        {
            var collection = (IServiceCollection) new ServiceCollection();
            var storeDependencies = collection.AddMicrowave(typeof(TestReadModelSubscriptions).Assembly);

            var buildServiceProvider = storeDependencies.BuildServiceProvider();

            var publishingEventRegistration = buildServiceProvider.GetServices<SubscribedEventCollection>().Single();
            var readModelSubscription = publishingEventRegistration.ReadModelSubcriptions.ToList();
            Assert.AreEqual(nameof(TestDomainEvent_PublishedEvent1), readModelSubscription[0].GetsCreatedOn);
            Assert.AreEqual(nameof(TestReadModelSubscriptions), readModelSubscription[0].ReadModelName);
            Assert.AreEqual(1, readModelSubscription.Count);
        }

        [TestMethod]
        public void AddMicrowaveDependencies_DepedencyControllerIsResolved()
        {
            var collection = (IServiceCollection) new ServiceCollection();
            var storeDependencies = collection.AddMicrowave(typeof(TestReadModelSubscriptions).Assembly);

            var buildServiceProvider = storeDependencies.BuildServiceProvider();

            var discoveryController = buildServiceProvider.GetServices<DiscoveryController>().Single();
            Assert.IsNotNull(discoveryController);
        }

        [TestMethod]
        public void AddMicrowaveDependencies_RunStarts_DiscoveryFails()
        {
            var collection = (IServiceCollection) new ServiceCollection();
            var storeDependencies = collection.AddMicrowave(new WriteModelConfiguration(), new ReadModelConfiguration
            {
                Database = new ReadDatabaseConfig
                {
                    DatabaseName = "IntegrationTest",
                },
                ServiceLocations = new ServiceBaseAddressCollection
                {
                    new Uri("http://localhost:5002")
                }
            }, typeof
                (TestReadModelSubscriptions).Assembly);

            var buildServiceProvider = storeDependencies.BuildServiceProvider();

            var discoveryHandler = buildServiceProvider.CreateScope().ServiceProvider.GetService<DiscoveryHandler>();

            discoveryHandler.DiscoverConsumingServices().Wait();

            var discoveryController = buildServiceProvider.GetServices<DiscoveryController>().Single();
            Assert.IsNotNull(discoveryController);
        }
    }

    public class TestIdQuery : ReadModel, IHandle<TestDomainEvent1>, IHandle<TestDomainEvent2>
    {
        public void Handle(TestDomainEvent1 domainEvent)
        {
        }

        public void Handle(TestDomainEvent2 domainEvent)
        {
        }

        public override Type GetsCreatedOn => typeof(TestDomainEvent1);
    }

    public class TestIdQuerySingle : ReadModel, IHandle<TestDomainEvent3>
    {
        public void Handle(TestDomainEvent3 domainEvent)
        {
        }

        public override Type GetsCreatedOn => typeof(TestDomainEvent3);
    }

    public class TestDomainEvent3 : IDomainEvent
    {
        public TestDomainEvent3(Identity entityId, int age)
        {
            EntityId = entityId;
            Age = age;
        }

        public Identity EntityId { get; }
        public int Age { get; }
    }

    public class TestIdQuery2 : ReadModel, IHandle<TestDomainEvent1>
    {
        public void Handle(TestDomainEvent1 domainEvent)
        {
        }

        public override Type GetsCreatedOn  => typeof(TestDomainEvent1);
    }

    public class TestQuery1 : Query, IHandle<TestDomainEvent1>, IHandle<TestDomainEvent2>
    {
        public void Handle(TestDomainEvent1 domainEvent)
        {
        }

        public void Handle(TestDomainEvent2 domainEvent)
        {
        }
    }

    public class TestQuery2 : Query, IHandle<TestDomainEvent1>
    {
        public void Handle(TestDomainEvent1 domainEvent)
        {
        }
    }

    public class TestEventHandler : IHandleAsync<TestDomainEvent1>, IHandleAsync<TestDomainEvent2>
    {
        public Task HandleAsync(TestDomainEvent1 domainEvent)
        {
            return Task.CompletedTask;
        }

        public Task HandleAsync(TestDomainEvent2 domainEvent)
        {
            return Task.CompletedTask;
        }
    }

    public class TestEventHandler2 : IHandleAsync<TestDomainEvent1>
    {
        public Task HandleAsync(TestDomainEvent1 domainEvent)
        {
            return  Task.CompletedTask;
        }
    }

    public class TestDomainEvent2 : IDomainEvent
    {
        public TestDomainEvent2(GuidIdentity entityId, string otherName)
        {
            EntityId = entityId;
            OtherName = otherName;
        }

        public Identity EntityId { get; }
        public string OtherName { get; }
    }

    public class TestDomainEvent1 : IDomainEvent
    {
        public TestDomainEvent1(GuidIdentity entityId, string name)
        {
            EntityId = entityId;
            Name = name;
        }

        public Identity EntityId { get; }
        public string Name { get; }
    }
}