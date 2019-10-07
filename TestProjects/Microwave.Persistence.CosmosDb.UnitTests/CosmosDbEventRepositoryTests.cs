using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain.EventSourcing;
using Microwave.Domain.Exceptions;
using Microwave.Domain.Identities;
using Microwave.Domain.Results;
using Microwave.EventStores;
using Microwave.EventStores.Ports;
using Microwave.Persistence.CosmosDb;

namespace Microwave.Persistence.CosmosDb.UnitTests
{
    [TestClass]
    public class CosmosDbEventRepositoryTests : IntegrationTests
    {
        [TestMethod]
        public async Task DomainEventIsAppendedCorrectly()
        {
            var cosmosDbClient = new CosmosDbClient(
                Database,
                new List<Assembly>
                {
                    Assembly.GetAssembly(typeof(UserCreatedEvent))
                });

            await Database.InitializeCosmosDb();

            var eventRepository = new CosmosDbEventRepository(cosmosDbClient, Database, new List<Assembly> { Assembly.GetAssembly(typeof(UserCreatedEvent)) });
            var domainEvent = new UserCreatedEvent(GuidIdentity.Create(Guid.NewGuid()), "Hans Wurst");
            await cosmosDbClient.CreateItemAsync(new DomainEventWrapper
            {
                Created = DateTimeOffset.Now,
                Version = 0,
                DomainEvent = domainEvent
            });

        }

        [TestMethod]
        public async Task DomainEventReturnsConcurrencyResult()
        {
            var cosmosDbClient = new CosmosDbClient(
                Database,
                new List<Assembly>
                {
                    Assembly.GetAssembly(typeof(UserCreatedEvent))
                });

            await Database.InitializeCosmosDb();
            var entityGuid = Guid.NewGuid();
            var eventRepository = new CosmosDbEventRepository(cosmosDbClient, Database, new List<Assembly> { Assembly.GetAssembly(typeof(UserCreatedEvent)) });
            var domainEvent = new UserCreatedEvent(GuidIdentity.Create(entityGuid), "Hans Wurst");
            await cosmosDbClient.CreateItemAsync(new DomainEventWrapper
            {
                Created = DateTimeOffset.Now,
                Version = 0,
                DomainEvent = domainEvent
            });
            var result = await cosmosDbClient.CreateItemAsync(new DomainEventWrapper
            {
                Created = DateTimeOffset.Now,
                Version = 0,
                DomainEvent = domainEvent
            });
            Assert.ThrowsException<ConcurrencyViolatedException>(() => result.Check());

        }

        [TestMethod]
        public async Task DomainEventsAreGettedCorrectly()
        {
            var cosmosDbClient = new CosmosDbClient(
                Database,
                new List<Assembly>
                {
                    Assembly.GetAssembly(typeof(UserCreatedEvent))
                });

            await Database.InitializeCosmosDb();
            var entityGuid = Guid.NewGuid();
            var domainEvent = new UserCreatedEvent(GuidIdentity.Create(entityGuid), "Hans Wurst");
            await cosmosDbClient.CreateItemAsync(new DomainEventWrapper
            {
                Created = DateTimeOffset.Now,
                Version = 0,
                DomainEvent = domainEvent
            });
            var eventRepository = new CosmosDbEventRepository(cosmosDbClient, Database, new List<Assembly>{ Assembly.GetAssembly(typeof(UserCreatedEvent)) } );
            var result = await cosmosDbClient.GetDomainEventsAsync(Identity.Create(entityGuid), 0);

            Assert.AreEqual(result.Count(), 1);
        }

        [TestMethod]
        public async Task SnapShotIsCreatedSuccesfully()
        {
            var cosmosDbClient = new CosmosDbClient(
                Database,
                new List<Assembly>
                {
                    Assembly.GetAssembly(typeof(UserCreatedEvent))
                });

            await Database.InitializeCosmosDb();

            await cosmosDbClient.SaveSnapshotAsync(new SnapShotWrapper<TestUser>(new TestUser
                {
                    UserName = "TestUser",
                    Age = 28
                },
                new GuidIdentity(Guid.NewGuid().ToString()), 1));
        }

        [TestMethod]
        public async Task SnapSHotAreGettedCorrectly()
        {
            var cosmosDbClient = new CosmosDbClient(
                Database,
                new List<Assembly>
                {
                    Assembly.GetAssembly(typeof(UserCreatedEvent))
                });

            await Database.InitializeCosmosDb();

            var eventRepository = new CosmosDbEventRepository(cosmosDbClient, Database, new List<Assembly> { Assembly.GetAssembly(typeof(UserCreatedEvent)) });

            var entityGuid = Guid.NewGuid();
            await cosmosDbClient.SaveSnapshotAsync(new SnapShotWrapper<TestUser>(new TestUser
                {
                    UserName = "TestUser",
                    Age = 28
                },
                new GuidIdentity(entityGuid.ToString()), 1));

            //var result = await eventRepository.LoadSnapshotAsync<TestUser>(Identity.Create(entityGuid));

            //Assert.AreEqual(result.Entity.Age, 28);
            //Assert.AreEqual(result.Entity.UserName, "TestUser");
        }
    }

    public class TestUser
    {
        public string UserName { get; set; }
        public int Age { get; set; }
    }

    public class UserCreatedEvent : IDomainEvent
    {
        public Identity EntityId { get; }
        public string Username { get; }

        public UserCreatedEvent(GuidIdentity entityId, string name)
        {
            EntityId = entityId;
            Username = name;
        }
    }
}
