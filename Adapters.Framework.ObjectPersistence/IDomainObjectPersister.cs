using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Framework;

namespace Adapters.Framework.ObjectPersistence
{
    public interface IDomainObjectPersister
    {
        Task Store(IEnumerable<DomainEvent> domainEvents);
        IEnumerable<DomainEvent> Load();
    }
}