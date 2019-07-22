using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microwave.Discovery;
using Microwave.EventStores;
using Microwave.EventStores.Ports;
using Microwave.Persistence.MongoDb.Eventstores;
using Microwave.Persistence.MongoDb.Querries;
using Microwave.Queries;

namespace Microwave.Persistence.MongoDb
{
    public class MongoDbPersistenceLayer : IPersistenceLayer
    {
        public MicrowaveMongoDb MicrowaveMongoDb { get; set; } = new MicrowaveMongoDb();

        public IServiceCollection AddPersistenceLayer(IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            services.AddTransient<IStatusRepository, StatusRepository>();

            services.AddTransient<IVersionRepository, VersionRepository>();
            services.AddTransient<IReadModelRepository, ReadModelRepository>();
            services.AddSingleton(MicrowaveMongoDb);
            services.AddSingleton<IEventLocationCache>(new EventLocationCache());

            services.AddTransient<IEventRepository, EventRepository>();
            services.AddSingleton<IVersionCache, VersionCache>();
            services.AddTransient<ISnapShotRepository, SnapShotRepository>();

            foreach (var assembly in assemblies)
            {
                BsonMapRegistrationHelpers.AddBsonMapsForMicrowave(assembly);
            }

            return services;
        }
    }

}