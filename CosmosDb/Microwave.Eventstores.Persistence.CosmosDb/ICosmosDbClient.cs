using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microwave.Domain.EventSourcing;
using Microwave.Domain.Identities;
using Microwave.Domain.Results;
using Microwave.EventStores;

namespace Microwave.Eventstores.Persistence.CosmosDb
{
    public interface ICosmosDbClient
    {
        Task CreateDomainEventAsync(IDomainEvent domainEvent);
        Task<Result<IEnumerable<DomainEventWrapper>>> GetDomainEventsAsync(DateTimeOffset tickSince);
        Task<Result<IEnumerable<DomainEventWrapper>>> LoadEventsByTypeAsync(string eventType, DateTimeOffset tickSince);
        Task<Document> CreateItemAsync(DomainEventWrapper domainEvent);
        Task<IEnumerable<IDomainEvent>> GetDomainEventsAsync(Identity identity);
    }
}