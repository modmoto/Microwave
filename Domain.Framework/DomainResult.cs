using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Framework
{
    public class DomainResult
    {
        private readonly IEnumerable<DomainEvent> _domainEvents;
        public Boolean IsOk => !DomainErrors.Any();
        public Boolean Failed => !IsOk;

        public IEnumerable<DomainEvent> DomainEvents
        {
            get
            {
                if (Failed) throw new DomainValidationException(DomainErrors);
                return _domainEvents;
            }
        }

        public IEnumerable<string> DomainErrors { get; }

        private DomainResult(IEnumerable<DomainEvent> domainEvents, IEnumerable<string> domainErrors)
        {
            _domainEvents = domainEvents;
            DomainErrors = domainErrors;
        }

        public static DomainResult Ok(IEnumerable<DomainEvent> domainEvents)
        {
            return new DomainResult(domainEvents, new List<string>());
        }

        public static DomainResult Ok(DomainEvent domainEvent)
        {
            return new DomainResult(new List<DomainEvent> { domainEvent }, new List<string>());
        }

        public static DomainResult Error(IEnumerable<string> domainErrors)
        {
            return new DomainResult(new List<DomainEvent>(), domainErrors);
        }

        public void EnsureSucces()
        {
            if (Failed) throw new DomainValidationException(DomainErrors);
        }
    }
}