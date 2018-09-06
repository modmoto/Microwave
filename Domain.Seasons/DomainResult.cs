using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Framework;

namespace Domain.Seasons
{
    public class DomainResult
    {
        public Boolean Ok => !DomainErrors.Any();
        public Boolean Failed => !Ok;

        public IEnumerable<DomainEvent> DomainEvents { get; }

        public IEnumerable<string> DomainErrors { get; }

        private DomainResult(IEnumerable<DomainEvent> domainEvents, IEnumerable<string> domainErrors)
        {
            DomainEvents = domainEvents;
            DomainErrors = domainErrors;
        }

        public static DomainResult OkResult(IEnumerable<DomainEvent> domainEvents)
        {
            return new DomainResult(domainEvents, new List<string>());
        }

        public static DomainResult OkResult(DomainEvent domainEvent)
        {
            return new DomainResult(new List<DomainEvent> { domainEvent }, new List<string>());
        }

        public static DomainResult ErrorResult(IEnumerable<string> domainErrors)
        {
            return new DomainResult(new List<DomainEvent>(), domainErrors);
        }
    }
}