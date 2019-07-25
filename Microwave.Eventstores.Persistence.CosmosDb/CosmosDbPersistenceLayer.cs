using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microwave.EventStores;

namespace Microwave.Persistence.CosmosDb
{
    public class CosmosDbPersistenceLayer : IPersistenceLayer
    {
        public MicrowaveCosmosDb MicrowaveCosmosDb { get; set; } = new MicrowaveCosmosDb();

        public IServiceCollection AddPersistenceLayer(IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            services.AddTransient<ICosmosDb, CosmosDb>();
            //services.AddTransient<IStatusRepository, CosmosDbStatusRepository>();

            //services.AddTransient<IVersionRepository, CosmosDbVersionRepository>();
            //services.AddTransient<IReadModelRepository, CosmosDbReadModelRepository>();
            //services.AddSingleton(MicrowaveCosmosDb);
            //services.AddSingleton(new CosmosDbEventLocationCache());

            services.AddTransient<IEventRepository, CosmosDbEventRepository>();
            //services.AddSingleton<IVersionCache, CosmosDbVersionCache>();
            //services.AddTransient<ISnapShotRepository, CosmosDbSnapShotRepository>();

            return services;
        }
    }

    public class MicrowaveCosmosDb
    {
        public string DatabaseUrl { get; set; }
        public string PrimaryKey { get; set; }
    }
}