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
            services.AddSingleton<IStatusRepository, StatusRepositoryInMemory>();
            services.AddSingleton<IVersionRepository, VersionRepositoryInMemory>();
            services.AddSingleton<IReadModelRepository, ReadModelRepositoryInMemory>();
            services.AddSingleton<IEventRepository, EventRepositoryInMemory>();
            services.AddSingleton<ISnapShotRepository, SnapShotRepositoryInMemory>();

            return services;
        }
    }
}