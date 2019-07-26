using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microwave.Domain.EventSourcing;
using Microwave.Domain.Identities;
using Microwave.Domain.Results;
using Microwave.EventStores;
using Microwave.EventStores.Ports;

namespace Microwave.Persistence.CosmosDb
{
    public interface ICosmosDbClient
    {
        Task<IEnumerable<DomainEventWrapper>> GetDomainEventsAsync(DateTimeOffset tickSince);
        Task<IEnumerable<DomainEventWrapper>> LoadEventsByTypeAsync(string eventType, DateTimeOffset tickSince);
        Task<Result> CreateItemAsync(DomainEventWrapper domainEvent);
        Task<IEnumerable<DomainEventWrapper>> GetDomainEventsAsync(Identity identity);
        Task<SnapShotResult<T>> LoadSnapshotAsync<T>(Identity entityId);
        Task SaveSnapshotAsync<T>(SnapShotWrapper<T> snapShot);
    }
}