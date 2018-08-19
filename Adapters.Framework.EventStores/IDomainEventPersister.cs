using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Framework;

namespace Adapters.Framework.EventStores
{
    public interface IDomainEventPersister
    {
        Task Store(IEnumerable<DomainEvent> domainEvents);
        IEnumerable<DomainEvent> Load();
    }
}