using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microwave.Configuration.MongoDb;
using Microwave.Discovery;
using Microwave.EventStores;
using Microwave.Eventstores.Persistence.MongoDb;
using Microwave.EventStores.Ports;
using Microwave.Queries;
using Microwave.Queries.Persistence.MongoDb;

namespace Microwave.Persistence.MongoDb.Extensions
{
    public class MongoDbPersistenceLayer : IPersistenceLayer
    {
        public IServiceCollection AddPersistenceLayer(IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            services.AddTransient<IStatusRepository, StatusRepository>();

            services.AddTransient<IVersionRepository, VersionRepository>();
            services.AddTransient<IReadModelRepository, ReadModelRepository>();
            services.AddTransient<MicrowaveDatabase>();
            services.AddSingleton(new EventLocationCache());

            services.AddTransient<IEventRepository, EventRepository>();
            services.AddSingleton<IVersionCache, VersionCache>();
            services.AddTransient<ISnapShotRepository, SnapShotRepository>();
            services.AddTransient<MicrowaveDatabase>();

            foreach (var assembly in assemblies)
            {
                BsonMapRegistrationHelpers.AddBsonMapsForMicrowave(assembly);
            }

            return services;
        }
    }

}