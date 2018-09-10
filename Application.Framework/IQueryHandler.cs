using System;
using System.Collections.Generic;
using Domain.Framework;

namespace Application.Framework
{
    public interface IQueryHandler
    {
        void Handle(DomainEvent domainEvent, long version);
        IEnumerable<Type> SubscribedTypes { get; }
        long LastSubscriptionVersion { get; }
    }
}