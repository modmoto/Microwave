using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microwave.Discovery;
using Microwave.Domain.EventSourcing;
using Microwave.EventStores.Ports;
using Microwave.Persistence.InMemory.Eventstores;
using Microwave.Persistence.InMemory.Querries;
using Microwave.Queries;
using Microwave.Queries.Ports;

namespace Microwave.Persistence.InMemory
{
    public static class InMemoryPersistenceExtensions
    {
        public static IServiceCollection AddMicrowavePersistenceLayerInMemory(this IServiceCollection services,
            Action<DomainEventSeeding> addEventSeeding = null)
        {
            addEventSeeding = addEventSeeding ?? (d => { });
            var domainEventSeeding = new DomainEventSeeding();
            addEventSeeding.Invoke(domainEventSeeding);

            var eventRepositoryInMemory = new EventRepositoryInMemory(domainEventSeeding.DomainEventSeeds);

            services.AddSingleton<IStatusRepository, StatusRepositoryInMemory>();
            services.AddSingleton<IVersionRepository, VersionRepositoryInMemory>();
            services.AddSingleton<IReadModelRepository, ReadModelRepositoryInMemory>();
            services.AddSingleton<IEventRepository>(eventRepositoryInMemory);
            services.AddSingleton<ISnapShotRepository, SnapShotRepositoryInMemory>();

            return services;
        }
    }

    public class DomainEventSeeding
    {
        public IEnumerable<IDomainEvent> DomainEventSeeds { get; private set; } = new List<IDomainEvent>();

        public DomainEventSeeding WithEventSeeds(IEnumerable<IDomainEvent> domainEvents)
        {
            var events = DomainEventSeeds.ToList();
            events.AddRange(domainEvents);
            DomainEventSeeds = events;
            return this;
        }

        public DomainEventSeeding WithEventSeeds(IDomainEvent domainEvent)
        {
            return WithEventSeeds(new List<IDomainEvent> { domainEvent });
        }
    }
}