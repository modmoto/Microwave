using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Framework
{
    public class DomainResult
    {
        private readonly IEnumerable<IDomainEvent> _domainEvents;
        public Boolean IsOk => !DomainErrors.Any();
        public Boolean Failed => !IsOk;

        public IEnumerable<IDomainEvent> DomainEvents
        {
            get
            {
                if (Failed) throw new DomainValidationException(DomainErrors);
                return _domainEvents;
            }
        }

        public IEnumerable<string> DomainErrors { get; }

        private DomainResult(IEnumerable<IDomainEvent> domainEvents, IEnumerable<string> domainErrors)
        {
            _domainEvents = domainEvents;
            DomainErrors = domainErrors;
        }

        public static DomainResult Ok(IEnumerable<IDomainEvent> domainEvents)
        {
            return new DomainResult(domainEvents, new List<string>());
        }

        public static DomainResult Ok(IDomainEvent domainEvent)
        {
            return new DomainResult(new List<IDomainEvent> { domainEvent }, new List<string>());
        }

        public static DomainResult Error(IEnumerable<string> domainErrors)
        {
            return new DomainResult(new List<IDomainEvent>(), domainErrors);
        }

        public void EnsureSucces()
        {
            if (Failed) throw new DomainValidationException(DomainErrors);
        }
    }
}