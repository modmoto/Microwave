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

            services.AddTransient<IStatusRepository, StatusRepository>();

            services.AddTransient<IVersionRepository, VersionRepository>();
            services.AddTransient<IReadModelRepository, ReadModelRepository>();
            services.AddSingleton(microwaveMongoDb);
            services.AddSingleton<IEventLocationCache>(new EventLocationCache());

            services.AddTransient<IEventRepository, EventRepository>();
            services.AddSingleton<IVersionCache, VersionCache>();
            services.AddTransient<ISnapShotRepository, SnapShotRepository>();

            foreach (var assembly in ServiceCollectionExtensions.GetAllAssemblies())
            {
                BsonMapRegistrationHelpers.AddBsonMapsForMicrowave(assembly);
            }

            return services;
        }
    }
}