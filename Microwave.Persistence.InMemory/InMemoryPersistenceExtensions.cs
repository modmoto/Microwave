using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microwave.Discovery;
using Microwave.Domain.EventSourcing;
using Microwave.EventStores.Ports;
using Microwave.Persistence.InMemory.Eventstores;
using Microwave.Persistence.InMemory.Querries;
using Microwave.Persistence.InMemory.Subscriptions;
using Microwave.Queries;
using Microwave.Queries.Ports;
using Microwave.Subscriptions.Ports;

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

            var eventRepositoryInMemory = new EventRepositoryInMemory();
            var groupedEvents = domainEventSeeding.DomainEventSeeds.GroupBy(de => de.EntityId).ToList();

            foreach (var domainEvents in groupedEvents)
            {
                var appendAsync = eventRepositoryInMemory.AppendAsync(domainEvents, 0);
                appendAsync.Wait();
                var res = appendAsync.Result;
                res.Check();
            }

            services.AddSingleton<IStatusRepository, StatusRepositoryInMemory>();
            services.AddSingleton<IVersionRepository, VersionRepositoryInMemory>();
            services.AddSingleton<IReadModelRepository, ReadModelRepositoryInMemory>();
            services.AddSingleton<IEventRepository>(eventRepositoryInMemory);
            services.AddSingleton<ISnapShotRepository, SnapShotRepositoryInMemory>();
            services.AddSingleton<IRemoteVersionReadModelRepository, RemoteVersionReadModelRepositoryInMemory>();
            services.AddSingleton<ISubscriptionRepository, SubscriptionRepositoryInMemory>();
            services.AddSingleton<IRemoteVersionRepository, RemoteVersionRepositoryInMemory>();
            services.AddSingleton<SharedMemoryClass>();

            return services;
        }
    }

    public class DomainEventSeeding
    {
        public IEnumerable<IDomainEvent> DomainEventSeeds { get; private set; } = new List<IDomainEvent>();

        public void WithEventSeeds(IEnumerable<IDomainEvent> domainEvents)
        {
            var events = DomainEventSeeds.ToList();
            events.AddRange(domainEvents);
            DomainEventSeeds = events;
        }

        public void WithEventSeed(IDomainEvent domainEvent)
        {
            var events = DomainEventSeeds.ToList();
            events.Add(domainEvent);
            DomainEventSeeds = events;
        }
    }
}