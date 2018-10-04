using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Framework;

namespace Application.Framework
{
    public interface IEventHandler
    {
        void Handle(DomainEvent domainEvent);
        IEnumerable<Type> SubscribedDomainEventTypes { get; }
    }
}