using System;
using System.Collections.Generic;
using Domain.Framework;

namespace Application.Framework
{
    public interface IQueryHandler
    {
        void Handle(DomainEvent domainEvent);
        IEnumerable<Type> SubscribedTypes { get; }
    }
}