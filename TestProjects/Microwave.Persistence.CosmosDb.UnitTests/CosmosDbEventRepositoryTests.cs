using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain.EventSourcing;
using Microwave.Domain.Identities;
using Microwave.EventStores;
using Microwave.Persistence.CosmosDb;

namespace Microwave.Persistence.CosmosDb.UnitTests
{
    [TestClass]
    public class CosmosDbEventRepositoryTests : IntegrationTests
    {
        [TestMethod]
        [Ignore]
        public async Task DomainEventIsAppendedCorrectly()
        {
            var cosmosDbClient = new CosmosDbClient(
                Database,
                new List<Assembly>
                {
                    Assembly.GetAssembly(typeof(UserCreatedEvent))
                });

            await cosmosDbClient.InitializeCosmosDbAsync();
           
            var eventRepository = new CosmosDbEventRepository(cosmosDbClient);
            var domainEvent = new UserCreatedEvent(GuidIdentity.Create(Guid.NewGuid()), "Hans Wurst");
            await cosmosDbClient.CreateItemAsync(new DomainEventWrapper
            {
                Created = DateTimeOffset.Now,
                Version = 0,
                DomainEvent = domainEvent
            });

        }

        [TestMethod]
        [Ignore]
        public async Task DomainEventsAreGettedCorrectly()
        {
            var cosmosDbClient = new CosmosDbClient(
                Database,
                new List<Assembly>
                {
                    Assembly.GetAssembly(typeof(UserCreatedEvent))
                });

            await cosmosDbClient.InitializeCosmosDbAsync();

            var eventRepository = new CosmosDbEventRepository(cosmosDbClient);
            var result = await cosmosDbClient.GetDomainEventsAsync(Identity.Create(Guid.Parse("19cf121a-44cd-40bb-a3b5-ea9deb11d4f5")));
        }
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
