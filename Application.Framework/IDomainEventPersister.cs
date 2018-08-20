using System.Collections.Generic;
using Domain.Framework;

namespace Application.Framework
{
    public interface IDomainEventPersister : IObjectPersister<IEnumerable<DomainEvent>>
    {
    }
}