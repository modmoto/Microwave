using System.Collections.Generic;
using Application.Framework;
using Domain.Framework;

namespace Adapters.Json.ObjectPersistences
{
    public class DomainEventPersister : ObjectPersister<IEnumerable<DomainEvent>>, IDomainEventPersister
    {
        public DomainEventPersister(string filePath) : base(filePath)
        {
        }
    }
}