using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Discovery;
using Microwave.Discovery.EventLocations;
using Microwave.Domain.EventSourcing;
using Microwave.EventStores;
using Microwave.Persistence.InMemory;
using Microwave.Persistence.MongoDb;
using Microwave.Queries;
using Microwave.Queries.Handler;
using Microwave.Queries.Polling;
using Microwave.Queries.Ports;
using Microwave.UnitTests.PublishedEventsDll;
using Microwave.WebApi;
using Microwave.WebApi.Discovery;
using Microwave.WebApi.Queries;

namespace Microwave.UnitTests
{
    [TestClass]
    public class ExtensionTests
    {
        [TestMethod]

        public void DuplicateDomainEventExceptionCreator()
        {
            var duplicateDomainEventException = new DuplicateDomainEventException(typeof(TestDomainEvent1));
            Assert.IsTrue(duplicateDomainEventException.Message.Contains(nameof(TestDomainEvent1)));
        }

        [TestMethod]
        public async Task AddDiContainerTest_WithSeeding()
        {
            var collection = (IServiceCollection) new ServiceCollection();

            var storeDependencies = collection
                .AddMicrowave(c => c.WithFeedType(typeof(EventFeed<>)))
                .AddMicrowaveWebApi()
                .AddMicrowavePersistenceLayerInMemory(s =>
                    s.WithEventSeeds(new TestDomainEvent_PublishedEvent2("testId"))
                    .WithEventSeeds(new TestDomainEvent_PublishedEvent2("testId2")));
            var buildServiceProvider = storeDependencies.BuildServiceProvider();

            var eventStore = buildServiceProvider.GetService<IEventStore>();
            var entity = await eventStore.LoadAsync<TestEntityForSeed>("testId");
            var entity2 = await eventStore.LoadAsync<TestEntityForSeed>("testId2");

            Assert.AreEqual("testId", entity.Value.DomainEventEntityId);
            Assert.AreEqual("testId2", entity2.Value.DomainEventEntityId);
        }

        [TestMethod]
        public void AddDiContainerTest_CorrectPollIntervalls()
        {
            var collection = (IServiceCollection) new ServiceCollection();

            var storeDependencies = collection
                .AddMicrowave()
                .AddMicrowaveWebApi()
                .AddMicrowavePersistenceLayerInMemory();
            var buildServiceProvider = storeDependencies.BuildServiceProvider();

            Assert.IsNotNull(buildServiceProvider.GetService<PollingInterval<AsyncEventHandler<TestEventHandler, TestDomainEvent1>>>());
            Assert.IsNotNull(buildServiceProvider.GetService<PollingInterval<ReadModelEventHandler<TestIdQuery2>>>());
            Assert.IsNotNull(buildServiceProvider.GetService<PollingInterval<QueryEventHandler<TestQuery1, TestDomainEvent1>>>());
            Assert.IsNotNull(buildServiceProvider.GetService<PollingInterval<QueryEventHandler<TestQuery1, TestDomainEvent2>>>());
            Assert.IsNull(buildServiceProvider.GetService<PollingInterval<QueryEventHandler<TestQuery2, TestDomainEvent2>>>());
        }

        [TestMethod]
        public void AddDiContainerTest()
        {
            var collection = (IServiceCollection) new ServiceCollection();

            var storeDependencies = collection
                .AddMicrowave(c => c.WithFeedType(typeof(EventFeed<>)))
                .AddMicrowaveWebApi()
                .AddMicrowavePersistenceLayerInMemory();
            var buildServiceProvider = storeDependencies.BuildServiceProvider();

            Assert.AreEqual(typeof(TestEventHandler), buildServiceProvider.GetService<AsyncEventHandler<TestEventHandler, TestDomainEvent1>>().HandlerClassType);
            Assert.AreEqual(typeof(TestEventHandler), buildServiceProvider.GetService<AsyncEventHandler<TestEventHandler, TestDomainEvent2>>().HandlerClassType);

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

            var eventFeeds = buildServiceProvider.GetServices<IEventFeed<AsyncEventHandler<TestEventHandler, TestDomainEvent1>>>().FirstOrDefault();
            var eventFeeds2 = buildServiceProvider.GetServices<IEventFeed<AsyncEventHandler<TestEventHandler, TestDomainEvent2>>>().FirstOrDefault();
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

            Assert.IsNotNull(buildServiceProvider.GetService<QueryEventHandler<TestQueryOnlyOneSubscribedEvent, TestDomainEvent_OnlySubscribedEventForList>>());
            Assert.IsNotNull(buildServiceProvider.GetService<QueryEventHandler<TestQueryOnlyOneSubscribedEvent, TestDomainEvent_OnlySubscribedEventForList>>());
            Assert.IsNotNull(buildServiceProvider.GetService<QueryEventHandler<TestQuery2, TestDomainEvent1>>());
            Assert.IsNotNull(buildServiceProvider.GetService<QueryEventHandler<TestQuery1, TestDomainEvent1>>());
            Assert.IsNotNull(buildServiceProvider.GetService<QueryEventHandler<TestQuery1, TestDomainEvent2>>());
            Assert.IsNotNull(buildServiceProvider.GetService<QueryEventHandler<TestQuerry_NotImplementingIApply, TestDomainEvent_OnlySubscribedEvent>>());

            Assert.IsNotNull(buildServiceProvider.GetService<ReadModelEventHandler<TestIdQuery>>());
            Assert.IsNotNull(buildServiceProvider.GetService<ReadModelEventHandler<TestIdQuery2>>());
            Assert.IsNotNull(buildServiceProvider.GetService<ReadModelEventHandler<TestIdQuerySingle>>());
            Assert.IsNotNull(buildServiceProvider.GetService<ReadModelEventHandler<TestReadModel>>());
            Assert.IsNotNull(buildServiceProvider.GetService<ReadModelEventHandler<TestReadModel_NotImplementingIApply>>());
            Assert.IsNotNull(buildServiceProvider.GetService<ReadModelEventHandler<TestReadModelSubscriptions>>());
        }

        [TestMethod]
        public void AddLocationsNotEmpty_ExtensionMethod()
        {
            var collection = (IServiceCollection) new ServiceCollection();

            var serviceBaseAddressCollection = new ServiceBaseAddressCollection
            {
                new Uri("http://ard.de")
            };
            var storeDependencies = collection
                .AddMicrowave(c => c.WithFeedType(typeof(EventFeed<>)))
                .AddMicrowaveWebApi(c =>
                {
                    c.WithServiceLocations(serviceBaseAddressCollection);
                })
                .AddMicrowavePersistenceLayerInMemory();

            var buildServiceProvider = storeDependencies.BuildServiceProvider();
            var addressesFromDi = buildServiceProvider.GetService<ServiceBaseAddressCollection>();

            Assert.AreEqual(1, addressesFromDi.Count);
        }

        [TestMethod]
        public void AddLocationsNotEmpty_AddOnList()
        {
            var collection = (IServiceCollection) new ServiceCollection();

            var storeDependencies = collection
                .AddMicrowave(c => c.WithFeedType(typeof(EventFeed<>)))
                .AddMicrowaveWebApi(c =>
                {
                    c.ServiceLocations.Add(new Uri("http://ard.de"));
                })
                .AddMicrowavePersistenceLayerInMemory();

            var buildServiceProvider = storeDependencies.BuildServiceProvider();
            var addressesFromDi = buildServiceProvider.GetService<ServiceBaseAddressCollection>();

            Assert.AreEqual(1, addressesFromDi.Count);
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

            var storeDependencies = collection
                .AddMicrowave(c => c.WithFeedType(typeof(EventFeed<>)))
                .AddMicrowaveWebApi()
                .AddMicrowavePersistenceLayerInMemory();
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
                .AddMicrowave(c => c.WithFeedType(typeof(EventFeed<>))).AddMicrowavePersistenceLayerInMemory()
                .AddMicrowave(c => c.WithFeedType(typeof(EventFeed<>))).AddMicrowavePersistenceLayerInMemory()
                .AddMicrowaveWebApi();

            var buildServiceProvider = storeDependencies.BuildServiceProvider();

            var eventFeed1 = buildServiceProvider.GetServices<IEventFeed<AsyncEventHandler<TestEventHandler, TestDomainEvent1>>>().FirstOrDefault();
            Assert.IsNotNull(eventFeed1); // double as just checking if no exception is done
        }

        [TestMethod]
        public void AddMicrowaveDependencies()
        {
            var collection = (IServiceCollection) new ServiceCollection();

            var storeDependencies = collection
                .AddMicrowave(c => c.WithFeedType(typeof(EventFeed<>)))
                .AddMicrowaveWebApi()
                .AddMicrowavePersistenceLayerInMemory();

            var buildServiceProvider = storeDependencies.BuildServiceProvider();

            var store = buildServiceProvider.GetServices<IEventStore>().FirstOrDefault();
            Assert.IsNotNull(store);
        }

        [TestMethod]
        public void AddMicrowaveDependencies_PollerIsRegistered()
        {
            var collection = (IServiceCollection) new ServiceCollection();

            var storeDependencies = collection
                .AddMicrowave()
                .AddMicrowaveWebApi()
                .AddMicrowavePersistenceLayerInMemory();

            var buildServiceProvider = storeDependencies.BuildServiceProvider();

            Assert.IsNotNull(buildServiceProvider.GetServices<DiscoveryPoller>());
        }

        [TestMethod]
        public void AddMicrowaveDependencies_PublishedEventsCorrect()
        {
            var collection = (IServiceCollection) new ServiceCollection();

            var storeDependencies = collection
                .AddMicrowave(c => c.WithFeedType(typeof(EventFeed<>)))
                .AddMicrowaveWebApi()
                .AddMicrowavePersistenceLayerInMemory();

            var buildServiceProvider = storeDependencies.BuildServiceProvider();

            var publishingEventRegistration = buildServiceProvider.GetServices<EventsPublishedByService>().Single()
            .PublishedEventTypes.OrderByDescending(e => e.Name).ToList();
            Assert.AreEqual(nameof(TestEntityThatShouldNotGoIntoReadModelRegistrationEvent), publishingEventRegistration[0].Name);
            Assert.AreEqual(nameof(TestDomainEvent_PublishedEvent2), publishingEventRegistration[1].Name);
            Assert.AreEqual(nameof(TestDomainEvent_PublishedEvent1), publishingEventRegistration[2].Name);
            Assert.AreEqual(nameof(Ev), publishingEventRegistration[4].Name);

            Assert.AreEqual(5, publishingEventRegistration.Count);

            var discoveryController = buildServiceProvider.GetServices<DiscoveryController>().Single();
            Assert.IsNotNull(discoveryController);
        }

        [TestMethod]
        public void AddMicrowaveDependencies_SubscribedEventsCorrect()
        {
            var collection = (IServiceCollection) new ServiceCollection();
            var storeDependencies = collection
                .AddMicrowave(c => c.WithFeedType(typeof(EventFeed<>)))
                .AddMicrowaveWebApi()
                .AddMicrowavePersistenceLayerInMemory();

            var buildServiceProvider = storeDependencies.BuildServiceProvider();

            var publishingEventRegistration = buildServiceProvider.GetServices<EventsSubscribedByService>().Single();
            var ihandleAsyncEvents = publishingEventRegistration.Events.OrderBy(r => r.Name).ToList();
            Assert.AreEqual(nameof(Ev), ihandleAsyncEvents[0].Name);
            Assert.AreEqual(nameof(TestDomainEvent_OnlySubscribedEvent), ihandleAsyncEvents[3].Name);
            Assert.AreEqual(nameof(TestDomainEvent_OnlySubscribedEvent_HandleAsync), ihandleAsyncEvents[4].Name);
            Assert.AreEqual(nameof(TestDomainEvent_OnlySubscribedEventForList), ihandleAsyncEvents[5].Name);
            Assert.AreEqual(nameof(TestDomainEvent_PublishedEvent1), ihandleAsyncEvents[6].Name);
            Assert.AreEqual(nameof(TestDomainEvent_PublishedEvent2), ihandleAsyncEvents[7].Name);


            Assert.AreEqual(11, ihandleAsyncEvents.Count);

            var discoveryController = buildServiceProvider.GetServices<DiscoveryController>().Single();
            Assert.IsNotNull(discoveryController);
        }

        [TestMethod]
        public void AddMicrowaveDependencies_ReadModelsCorrect()
        {
            var collection = (IServiceCollection) new ServiceCollection();
            var storeDependencies = collection.AddMicrowaveWebApi();

            var buildServiceProvider = storeDependencies.BuildServiceProvider();

            var publishingEventRegistration = buildServiceProvider.GetServices<EventsSubscribedByService>().Single();
            var readModelSubscription = publishingEventRegistration.ReadModelSubcriptions.OrderBy(r => r.ReadModelName).ToList();
            Assert.AreEqual(nameof(TestDomainEvent_PublishedEvent1), readModelSubscription[7].GetsCreatedOn.Name);
            Assert.AreEqual(nameof(TestReadModelSubscriptions), readModelSubscription[7].ReadModelName);
            Assert.AreEqual(8, readModelSubscription.Count);
        }

        [TestMethod]
        public void AddMicrowaveDependencies_DepedencyControllerIsResolved()
        {
            var collection = (IServiceCollection) new ServiceCollection();
            var storeDependencies = collection.
                AddMicrowave(c => c.WithFeedType(typeof(EventFeed<>)))
                .AddMicrowaveWebApi()
                .AddMicrowavePersistenceLayerInMemory();

            var buildServiceProvider = storeDependencies.BuildServiceProvider();

            var discoveryController = buildServiceProvider.GetServices<DiscoveryController>().Single();
            Assert.IsNotNull(discoveryController);
        }

        [TestMethod]
        public void AddMicrowaveDependencies_OnlyMicrowaveWithLocalEventFeed()
        {
            var collection = (IServiceCollection) new ServiceCollection();
            var storeDependencies = collection.
                AddMicrowave(c => c.WithFeedType(typeof(LocalEventFeed<>)))
                .AddMicrowavePersistenceLayerInMemory();

            var buildServiceProvider = storeDependencies.BuildServiceProvider();

            Assert.IsTrue(buildServiceProvider.GetServices<IHostedService>().Count() > 1);
        }
        
        [TestMethod]
        public void AddMicrowaveDependencies_ServiceNameIsCorrect()
        {
            var collection = (IServiceCollection) new ServiceCollection();
            var storeDependencies = collection.AddMicrowaveWebApi(config =>
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
            var storeDependencies = collection.AddMicrowaveWebApi(config =>
            {
                config.ServiceLocations.Add(new Uri("http://localhost:1234"));
            }).AddMicrowave(c => c.WithFeedType(typeof(EventFeed<>)));

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

    public class TestIdQuery : ReadModel<TestDomainEvent1>, IHandle<TestDomainEvent1>, IHandle<TestDomainEvent2>
    {
        public void Handle(TestDomainEvent1 domainEvent)
        {
        }

        public void Handle(TestDomainEvent2 domainEvent)
        {
        }
    }

    public class TestIdQuerySingle : ReadModel<TestDomainEvent3>, IHandle<TestDomainEvent3>
    {
        public void Handle(TestDomainEvent3 domainEvent)
        {
        }
    }

    public class TestDomainEvent3 : ISubscribedDomainEvent
    {
        public TestDomainEvent3(string entityId, int age)
        {
            EntityId = entityId;
            Age = age;
        }

        public string EntityId { get; }
        public int Age { get; }
    }

    public class TestIdQuery2 : ReadModel<TestDomainEvent1>, IHandle<TestDomainEvent1>
    {
        public void Handle(TestDomainEvent1 domainEvent)
        {
        }
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

    public class TestQueryOnlyOneSubscribedEvent : Query, IHandle<TestDomainEvent_OnlySubscribedEventForList>
    {
        public void Handle(TestDomainEvent_OnlySubscribedEventForList domainEvent)
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
        public TestDomainEvent2(string entityId, string otherName)
        {
            EntityId = entityId;
            OtherName = otherName;
        }

        public string EntityId { get; }
        public string OtherName { get; }
    }

    public class TestDomainEvent1 : ISubscribedDomainEvent
    {
        public TestDomainEvent1(string entityId, string name)
        {
            EntityId = entityId;
            Name = name;
        }

        public string EntityId { get; }
        public string Name { get; }
    }

    public class TestReadModel : ReadModel<Ev>, IHandle<Ev>
    {
        public void Handle(Ev domainEvent)
        {
        }
    }

    public class Ev : IDomainEvent, ISubscribedDomainEvent
    {
        public Ev(string entityId)
        {
            EntityId = entityId;
        }

        public string EntityId { get; }
    }

    public class TestDomainEvent_OnlySubscribedEventForList : ISubscribedDomainEvent
    {
        public TestDomainEvent_OnlySubscribedEventForList(string entityId)
        {
            EntityId = entityId;
        }

        public string EntityId { get; }
    }
}