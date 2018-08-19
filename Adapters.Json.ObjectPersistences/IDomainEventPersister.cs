using System.Collections.Generic;
using Domain.Framework;

namespace Adapters.Json.ObjectPersistences
{
    public interface IDomainEventPersister : IObjectPersister<IEnumerable<DomainEvent>>
    {
    }
}