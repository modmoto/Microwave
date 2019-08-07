using System;
using Microsoft.Extensions.DependencyInjection;
using Microwave.Discovery;
using Microwave.EventStores.Ports;
using Microwave.Persistence.MongoDb.Eventstores;
using Microwave.Persistence.MongoDb.Querries;
using Microwave.Queries;
using Microwave.Queries.Ports;

namespace Microwave.Persistence.MongoDb
{
    public static class MongoDbPersistenceExtensions
    {
        public static IServiceCollection AddMicrowavePersistenceLayerMongoDb(
            this IServiceCollection services,
            Action<MicrowaveMongoDb> mongoDb = null)
        {
            var action = mongoDb ?? (c => { });
            var microwaveMongoDb = new MicrowaveMongoDb();
            action.Invoke(microwaveMongoDb);

            services.AddTransient<IStatusRepository, StatusRepositoryMongoDb>();

            services.AddTransient<IVersionRepository, VersionRepositoryMongoDb>();
            services.AddTransient<IReadModelRepository, ReadModelRepositoryMongoDb>();
            services.AddSingleton(microwaveMongoDb);
            services.AddSingleton<IEventLocationCache>(new EventLocationCache());

            services.AddTransient<IEventRepository, EventRepositoryMongoDb>();
            services.AddSingleton<IVersionCache, VersionCache>();
            services.AddTransient<ISnapShotRepository, SnapShotRepositoryMongoDb>();

            foreach (var assembly in MicrowaveExtensions.GetAllAssemblies())
            {
                BsonMapRegistrationHelpers.AddBsonMapsForMicrowave(assembly);
            }

            return services;
        }
    }
}