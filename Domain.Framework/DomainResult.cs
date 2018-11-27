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

        public IEnumerable<DomainError> DomainErrors { get; }

        private DomainResult(IEnumerable<IDomainEvent> domainEvents, IEnumerable<DomainError> domainErrors)
        {
            _domainEvents = domainEvents;
            DomainErrors = domainErrors;
        }

        public static DomainResult Ok(IEnumerable<IDomainEvent> domainEvents)
        {
            return new DomainResult(domainEvents, new List<DomainError>());
        }

        public static DomainResult Ok(IDomainEvent domainEvent)
        {
            return new DomainResult(new List<IDomainEvent> { domainEvent }, new List<DomainError>());
        }

        public static DomainResult Error(DomainError domainDomainError)
        {
            return new DomainResult(new List<IDomainEvent>(), new List<DomainError> { domainDomainError });
        }

        public static DomainResult Error(IEnumerable<DomainError> domainErrors)
        {
            return new DomainResult(new List<IDomainEvent>(), domainErrors);
        }

        public void EnsureSucces()
        {
            if (Failed) throw new DomainValidationException(DomainErrors);
        }
    }
}