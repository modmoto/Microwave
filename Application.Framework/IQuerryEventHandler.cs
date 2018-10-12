using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Framework;

namespace Application.Framework
{
    public interface IQuerryEventHandler
    {
        Task Handle(DomainEvent domainEvent);
        IEnumerable<string> SubscribedDomainEventTypes { get; }
    }

    public interface IReactiveEventHandler
    {
        Task Handle(DomainEvent domainEvent, StreamVersion streamVersion);
        IEnumerable<string> SubscribedDomainEventTypes { get; }
    }
}