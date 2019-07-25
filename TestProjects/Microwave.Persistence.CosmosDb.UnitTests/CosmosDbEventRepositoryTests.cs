using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain.EventSourcing;
using Microwave.Domain.Identities;
using Microwave.EventStores;
using Microwave.Persistence.CosmosDb;

namespace Microwave.Persistence.CosmosDb.UnitTests
{
    [TestClass]
    public class CosmosDbEventRepositoryTests
    {
        [TestMethod]
        public async Task DomainEventIsAppendedCorrectly()
        {
            var databaseConfig = new MicrowaveCosmosDb();
            databaseConfig.DatabaseUrl = "https://spoppinga.documents.azure.com:443/";
            databaseConfig.PrimaryKey =
                "mCPtXM99gxlUalpz6bkFiWib2QD2OvIB9oEYj8tlpCPz1I4jSkOzlhJGnxAAEH4uiqWiYZ7enElzAM0lopKlJA==";
            
            var cosmosDatabse = new CosmosDb(databaseConfig);
            var cosmosDbClient = new CosmosDbClient(cosmosDatabse, new List<Assembly> { Assembly.GetAssembly(typeof(UserCreatedEvent)) });
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
        public async Task DomainEventsAreGettedCorrectly()
        {
            var databaseConfig = new MicrowaveCosmosDb();
            databaseConfig.DatabaseUrl = "https://spoppinga.documents.azure.com:443/";
            databaseConfig.PrimaryKey =
                "mCPtXM99gxlUalpz6bkFiWib2QD2OvIB9oEYj8tlpCPz1I4jSkOzlhJGnxAAEH4uiqWiYZ7enElzAM0lopKlJA==";

            var cosmosDatabse = new CosmosDb(databaseConfig);
            var cosmosDbClient = new CosmosDbClient(cosmosDatabse , new List<Assembly>{Assembly.GetAssembly(typeof(UserCreatedEvent))});
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
