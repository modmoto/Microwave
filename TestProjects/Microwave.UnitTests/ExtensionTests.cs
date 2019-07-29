using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Discovery;
using Microwave.Discovery.EventLocations;
using Microwave.Domain.EventSourcing;
using Microwave.Domain.Identities;
using Microwave.EventStores;
using Microwave.Persistence.MongoDb;
using Microwave.Queries;
using Microwave.Queries.Handler;
using Microwave.Queries.Ports;
using Microwave.UnitTests.PublishedEventsDll;
using Microwave.WebApi.Discovery;
using Microwave.WebApi.Queries;
using MongoDB.Bson.Serialization;

namespace Microwave.UnitTests
{
    [TestClass]
    public class ExtensionTests
    {
        [TestMethod]
        public void AddDiContainerTest()
        {
            var collection = (IServiceCollection) new ServiceCollection();

            var storeDependencies = collection.AddMicrowave().AddMicrowavePersistenceLayerMongoDb();
            var buildServiceProvider = storeDependencies.BuildServiceProvider();

            var eventDelegateHandlers = buildServiceProvider.GetServices<IAsyncEventHandler>().OrderBy(
                s => s.HandlerClassType.ToString()).ToList();
            Assert.AreEqual(7, eventDelegateHandlers.Count);

            Assert.AreEqual(typeof(TestEventHandler), eventDelegateHandlers[4].HandlerClassType);
            Assert.AreEqual(typeof(TestEventHandler), eventDelegateHandlers[5].HandlerClassType);
            Assert.AreEqual(typeof(TestEventHandler2), eventDelegateHandlers[6].HandlerClassType);

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

            var eventOverallClients1 = buildServiceProvider.GetServices<IEventFeed<ReadModelEventHandler<TestReadModel>>>().FirstOrDefault();
            var eventOverallClients2 = buildServiceProvider.GetServices<IEventFeed<ReadModelEventHandler<TestIdQuery>>>().FirstOrDefault();
            var eventOverallClients3 = buildServiceProvider.GetServices<IEventFeed<ReadModelEventHandler<TestIdQuerySingle>>>().FirstOrDefault();
            var eventOverallClients4 = buildServiceProvider.GetServices<IEventFeed<ReadModelEventHandler<TestIdQuery2>>>().FirstOrDefault();
            Assert.IsTrue(eventOverallClients1 is EventFeed<ReadModelEventHandler<TestReadModel>>);
            Assert.IsTrue(eventOverallClients2 is EventFeed<ReadModelEventHandler<TestIdQuery>>);
            Assert.IsTrue(eventOverallClients3 is EventFeed<ReadModelEventHandler<TestIdQuerySingle>>);
            Assert.IsTrue(eventOverallClients4 is EventFeed<ReadModelEventHandler<TestIdQuery2>>);

            var queryEventHandlers = buildServiceProvider.GetServices<IQueryEventHandler>().ToList();
            var qHandler1 = queryEventHandlers.OrderByDescending(e => e
            .GetType().GetGenericArguments().First().Name).ToList();
            Assert.AreEqual(4, qHandler1.Count);
            Assert.IsTrue(qHandler1[0] is QueryEventHandler<TestQuery2, TestDomainEvent1>);
            Assert.IsTrue(qHandler1[1] is QueryEventHandler<TestQuery1, TestDomainEvent1>);
            Assert.IsTrue(qHandler1[2] is QueryEventHandler<TestQuery1, TestDomainEvent2>);
            Assert.IsTrue(qHandler1[3] is QueryEventHandler<TestQuerry_NotImplementingIApply, TestDomainEvent_OnlySubscribedEvent>);

            var identHandler = buildServiceProvider.GetServices<IReadModelEventHandler>().OrderBy(r => r.GetType()
                .GetGenericArguments().First().Name).ToList();
            Assert.AreEqual(6, identHandler.Count);

            Assert.IsTrue(identHandler[0] is ReadModelEventHandler<TestIdQuery>);
            Assert.IsTrue(identHandler[1] is ReadModelEventHandler<TestIdQuery2>);
            Assert.IsTrue(identHandler[2] is ReadModelEventHandler<TestIdQuerySingle>);
            Assert.IsTrue(identHandler[3] is ReadModelEventHandler<TestReadModel>);
            Assert.IsTrue(identHandler[4] is ReadModelEventHandler<TestReadModel_NotImplementingIApply>);
            Assert.IsTrue(identHandler[5] is ReadModelEventHandler<TestReadModelSubscriptions>);
        }

        [TestMethod]
        public void EventRegistrationIsCorrect_OneAssembly()
        {
            var collection = (IServiceCollection) new ServiceCollection();

            var eventRegister = new EventRegistration();
            var storeDependencies = collection.AddDomainEventRegistration(typeof(TestDomainEvent1).Assembly, eventRegister);

            Assert.AreEqual(eventRegister[nameof(TestDomainEvent1)], typeof(TestDomainEvent1)); // IHandleAsyncEvent
            Assert.AreEqual(eventRegister[nameof(TestDomainEvent2)], typeof(TestDomainEvent2)); // QuerryEvent
        }

        [TestMethod]
        public void EventRegistrationIsCorrect()
        {
            var collection = (IServiceCollection) new ServiceCollection();

            var storeDependencies = collection.AddMicrowave().AddMicrowavePersistenceLayerMongoDb();
            var buildServiceProvider = storeDependencies.BuildServiceProvider();

            var eventRegister = buildServiceProvider.GetServices<EventRegistration>().Single();
            Assert.AreEqual(eventRegister[nameof(TestDomainEvent_PublishedEvent1)], typeof(TestDomainEvent_PublishedEvent1));
            Assert.AreEqual(eventRegister[nameof(TestDomainEvent_PublishedEvent2)], typeof(TestDomainEvent_PublishedEvent2));

            Assert.AreEqual(eventRegister[nameof(TestDomainEvent1)], typeof(TestDomainEvent1)); // IHandleAsyncEvent
            Assert.AreEqual(eventRegister[nameof(TestDomainEvent2)], typeof(TestDomainEvent2)); // QuerryEvent
        }

        [TestMethod]
        public void AddDiContainerTest_Twice()
        {
            var collection = (IServiceCollection) new ServiceCollection();

            var storeDependencies = collection
                .AddMicrowave().AddMicrowavePersistenceLayerMongoDb()
                .AddMicrowave().AddMicrowavePersistenceLayerMongoDb();

            var buildServiceProvider = storeDependencies.BuildServiceProvider();

            var eventFeed1 = buildServiceProvider.GetServices<IEventFeed<AsyncEventHandler<TestDomainEvent1>>>().FirstOrDefault();
            Assert.IsNotNull(eventFeed1);
            var identHandler = buildServiceProvider.GetServices<IReadModelEventHandler>().OrderBy(r => r.GetType()
            .GetGenericArguments().First().Name).ToList();
            Assert.IsTrue(identHandler[0] is ReadModelEventHandler<TestIdQuery>);
            Assert.AreEqual(12, identHandler.Count); // double as just checking if no exception is done
        }

        [TestMethod]
        public void AddMicrowaveDependencies()
        {
            var collection = (IServiceCollection) new ServiceCollection();

            var storeDependencies = collection.AddMicrowave().AddMicrowavePersistenceLayerMongoDb();

            var buildServiceProvider = storeDependencies.BuildServiceProvider();

            var store = buildServiceProvider.GetServices<IEventStore>().FirstOrDefault();
            Assert.IsNotNull(store);

            Assert.IsTrue(BsonClassMap.IsClassMapRegistered(typeof(TestDomainEvent_PublishedEvent1)));
            Assert.IsTrue(BsonClassMap.IsClassMapRegistered(typeof(TestDomainEvent_PublishedEvent2)));
        }

        [TestMethod]
        public void AddMicrowaveDependencies_PublishedEventsCorrect()
        {
            var collection = (IServiceCollection) new ServiceCollection();

            var storeDependencies = collection.AddMicrowave().AddMicrowavePersistenceLayerMongoDb();

            var buildServiceProvider = storeDependencies.BuildServiceProvider();

            var publishingEventRegistration = buildServiceProvider.GetServices<EventsPublishedByService>().Single()
            .PublishedEventTypes.OrderByDescending(e => e.Name).ToList();
            Assert.AreEqual(nameof(TestEntityThatShouldNotGoIntoReadModelRegistrationEvent), publishingEventRegistration[0].Name);
            Assert.AreEqual(nameof(TestDomainEvent_PublishedEvent2), publishingEventRegistration[1].Name);
            Assert.AreEqual(nameof(TestDomainEvent_PublishedEvent1), publishingEventRegistration[2].Name);
            Assert.AreEqual(nameof(Ev), publishingEventRegistration[3].Name);

            Assert.AreEqual(4, publishingEventRegistration.Count);

            var discoveryController = buildServiceProvider.GetServices<DiscoveryController>().Single();
            Assert.IsNotNull(discoveryController);
        }

        [TestMethod]
        public void AddMicrowaveDependencies_SubscribedEventsCorrect()
        {
            var collection = (IServiceCollection) new ServiceCollection();
            var storeDependencies = collection.AddMicrowave().AddMicrowavePersistenceLayerMongoDb();

            var buildServiceProvider = storeDependencies.BuildServiceProvider();

            var publishingEventRegistration = buildServiceProvider.GetServices<EventsSubscribedByService>().Single();
            var ihandleAsyncEvents = publishingEventRegistration.Events.OrderBy(r => r.Name).ToList();
            Assert.AreEqual(nameof(TestDomainEvent_OnlySubscribedEvent_HandleAsync), ihandleAsyncEvents[0].Name);
            Assert.AreEqual(nameof(TestDomainEvent_PublishedEvent1), ihandleAsyncEvents[1].Name);
            Assert.AreEqual(nameof(TestDomainEvent_PublishedEvent2), ihandleAsyncEvents[2].Name);
            Assert.AreEqual(nameof(TestDomainEvent1), ihandleAsyncEvents[3].Name);
            Assert.AreEqual(nameof(TestDomainEvent2), ihandleAsyncEvents[4].Name);
            Assert.AreEqual(5, ihandleAsyncEvents.Count);

            var discoveryController = buildServiceProvider.GetServices<DiscoveryController>().Single();
            Assert.IsNotNull(discoveryController);
        }

        [TestMethod]
        public void AddMicrowaveDependencies_ReadModelsCorrect()
        {
            var collection = (IServiceCollection) new ServiceCollection();
            var storeDependencies = collection.AddMicrowave();

            var buildServiceProvider = storeDependencies.BuildServiceProvider();

            var publishingEventRegistration = buildServiceProvider.GetServices<EventsSubscribedByService>().Single();
            var readModelSubscription = publishingEventRegistration.ReadModelSubcriptions.OrderBy(r => r.ReadModelName).ToList();
            Assert.AreEqual(nameof(TestDomainEvent_PublishedEvent1), readModelSubscription[5].GetsCreatedOn.Name);
            Assert.AreEqual(nameof(TestReadModelSubscriptions), readModelSubscription[5].ReadModelName);
            Assert.AreEqual(6, readModelSubscription.Count);
        }

        [TestMethod]
        public void AddMicrowaveDependencies_DepedencyControllerIsResolved()
        {
            var collection = (IServiceCollection) new ServiceCollection();
            var storeDependencies = collection.AddMicrowave().AddMicrowavePersistenceLayerMongoDb();

            var buildServiceProvider = storeDependencies.BuildServiceProvider();

            var discoveryController = buildServiceProvider.GetServices<DiscoveryController>().Single();
            Assert.IsNotNull(discoveryController);
        }
        
        [TestMethod]
        public void AddMicrowaveDependencies_ServiceNameIsCorrect()
        {
            var collection = (IServiceCollection) new ServiceCollection();
            var storeDependencies = collection.AddMicrowave(config =>
            {
                config.WithServiceName("TestService");
            });

            collection.AddMicrowavePersistenceLayerMongoDb(p =>
            {
                p.WithConnectionString("mongodb+srv://mongoDbTestUser:meinTestPw@cluster0-xhbcb.azure.mongodb.net/test?retryWrites=true&w=majority");
                p.WithDatabaseName("MicrowaveIntegrationTest");
            });

            var buildServiceProvider = storeDependencies.BuildServiceProvider();

            var services = buildServiceProvider.GetServices<EventsPublishedByService>().Single();
            
            Assert.AreEqual("TestService", services.ServiceEndPoint.Name);
        }

        [TestMethod]
        public async Task AddMicrowaveDependencies_RunStarts_DiscoveryFails()
        {
            var collection = (IServiceCollection) new ServiceCollection();
            var storeDependencies = collection.AddMicrowave(config =>
            {
                config.ServiceLocations.Add(new Uri("http://localhost:1234"));
            });

            collection.AddMicrowavePersistenceLayerMongoDb(p =>
            {
                p.WithConnectionString("mongodb+srv://mongoDbTestUser:meinTestPw@cluster0-xhbcb.azure.mongodb.net/test?retryWrites=true&w=majority");
                p.WithDatabaseName("MicrowaveIntegrationTest");
            });

            var buildServiceProvider = storeDependencies.BuildServiceProvider();

            var discoveryHandler = buildServiceProvider.CreateScope().ServiceProvider.GetService<IDiscoveryHandler>();

            await discoveryHandler.DiscoverConsumingServices();

            var discoveryController = buildServiceProvider.GetServices<DiscoveryController>().Single();
            Assert.IsNotNull(discoveryController);
        }
    }

    public class EntityWithSameDomainEvent : Entity, IApply<TestDomainEvent_PublishedEvent1>
    {
        public void Apply(TestDomainEvent_PublishedEvent1 domainEvent)
        {
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

    public class TestDomainEvent3 : ISubscribedDomainEvent
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

    public class TestDomainEvent2 : ISubscribedDomainEvent
    {
        public TestDomainEvent2(GuidIdentity entityId, string otherName)
        {
            EntityId = entityId;
            OtherName = otherName;
        }

        public Identity EntityId { get; }
        public string OtherName { get; }
    }

    public class TestDomainEvent1 : ISubscribedDomainEvent
    {
        public TestDomainEvent1(GuidIdentity entityId, string name)
        {
            EntityId = entityId;
            Name = name;
        }

        public Identity EntityId { get; }
        public string Name { get; }
    }

    public class TestReadModel : ReadModel, IHandle<Ev>
    {
        public void Handle(Ev domainEvent)
        {
        }

        public override Type GetsCreatedOn => typeof(Ev);
    }

    public class Ev : IDomainEvent, ISubscribedDomainEvent
    {
        public Ev(Identity entityId)
        {
            EntityId = entityId;
        }

        public Identity EntityId { get; }
    }
}