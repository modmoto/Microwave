using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microwave.Discovery;
using Microwave.Discovery.Subscriptions;
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
            services.AddTransient<IRemoteVersionRepository, RemoteVersionRepositoryMongoDb>();
            services.AddTransient<ISubscriptionRepository, SubscriptionRepositoryMongoDb>();

            foreach (var assembly in GetAllAssemblies())
            {
                BsonMapRegistrationHelpers.AddBsonMapsForMicrowave(assembly);
            }

            return services;
        }

        private static List<Assembly> GetAllAssemblies()
        {
            var assemblies = new List<Assembly>();
            var referencedPaths = Directory
                .GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll", SearchOption.AllDirectories).ToList();
            referencedPaths.ForEach(path =>
            {
                try
                {
                    var assemblyName = AssemblyName.GetAssemblyName(path);
                    assemblies.Add(AppDomain.CurrentDomain.Load(assemblyName));
                }
                catch (FileNotFoundException)
                {
                }
            });
            return assemblies;
        }
    }
}