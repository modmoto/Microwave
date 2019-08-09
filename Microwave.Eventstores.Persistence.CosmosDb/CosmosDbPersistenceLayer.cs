using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microwave.Discovery;
using Microwave.EventStores;
using Microwave.EventStores.Ports;
using Microwave.Queries;

namespace Microwave.Persistence.CosmosDb
{
    public class CosmosDbPersistenceLayer : IPersistenceLayer
    {
        public MicrowaveCosmosDb MicrowaveCosmosDb { get; set; } = new MicrowaveCosmosDb();

        public IServiceCollection AddPersistenceLayer(IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            services.AddTransient<ICosmosDb, CosmosDb>();
            var cosmosDb = new CosmosDb();
            cosmosDb.InitializeCosmosDb().Wait();

            services.AddTransient<IStatusRepository, CosmosDbStatusRepository>();
            services.AddTransient<IReadModelRepository, CosmosDbReadModelRepository>();
            services.AddSingleton(MicrowaveCosmosDb);

            services.AddTransient<IEventRepository, CosmosDbEventRepository>();
            services.AddTransient<ISnapShotRepository, CosmosDbSnapshotRepository>();

            return services;
        }
    }

    public class MicrowaveCosmosDb
    {
        public string DatabaseUrl { get; set; }
        public string PrimaryKey { get; set; }
    }
}