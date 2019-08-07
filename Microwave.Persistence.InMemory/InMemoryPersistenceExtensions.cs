using Microsoft.Extensions.DependencyInjection;
using Microwave.Discovery;
using Microwave.EventStores.Ports;
using Microwave.Persistence.InMemory.Eventstores;
using Microwave.Persistence.InMemory.Querries;
using Microwave.Queries;
using Microwave.Queries.Ports;

namespace Microwave.Persistence.InMemory
{
    public static class InMemoryPersistenceExtensions
    {
        public static IServiceCollection AddMicrowavePersistenceLayerInMemory(
            this IServiceCollection services)
        {
            services.AddTransient<IStatusRepository, StatusRepositoryInMemory>();

            services.AddTransient<IVersionRepository, VersionRepositoryInMemory>();
            services.AddTransient<IReadModelRepository, ReadModelRepositoryInMemory>();

            services.AddTransient<IEventRepository, EventRepositoryInMemory>();
            services.AddTransient<ISnapShotRepository, SnapShotRepositoryInMemory>();

            return services;
        }
    }
}