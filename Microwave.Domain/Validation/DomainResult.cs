using System;
using System.Collections.Generic;
using System.Linq;
using Microwave.Domain.EventSourcing;

namespace Microwave.Domain.Validation
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

        private DomainResult(IEnumerable<IDomainEvent> domainEvents)
        {
            _domainEvents = domainEvents;
            DomainErrors = new List<DomainError>();
        }

        private DomainResult(IEnumerable<DomainError> domainErrors)
        {
            _domainEvents = new List<IDomainEvent>();
            DomainErrors = domainErrors;
        }

        public static DomainResult Ok(IDomainEvent domainEvent)
        {
            return new DomainResult(new List<IDomainEvent> { domainEvent });
        }

        public static DomainResult Ok(IEnumerable<IDomainEvent> domainEvents)
        {
            return new DomainResult(domainEvents);
        }

        public static DomainResult Error(DomainError domainDomainError)
        {
            return new DomainResult(new List<DomainError> { domainDomainError });
        }

        public static DomainResult Error(string domainDomainErrorKey)
        {
            var domainError = new TypelessDomainError(domainDomainErrorKey);
            return new DomainResult(new List<DomainError> { domainError });
        }

        public static DomainResult Error(IEnumerable<string> domainDomainErrorKeys)
        {
            var enumDomainErrors = domainDomainErrorKeys.Select(e => new TypelessDomainError(e));
            return new DomainResult(enumDomainErrors);
        }

        public static DomainResult Error(Enum domainDomainErrorKey)
        {
            var domainError = new EnumDomainError(domainDomainErrorKey);
            return new DomainResult(new List<DomainError> { domainError });
        }

        public static DomainResult Error(IEnumerable<Enum> domainDomainErrorKeys)
        {
            var enumDomainErrors = domainDomainErrorKeys.Select(e => new EnumDomainError(e));
            return new DomainResult(enumDomainErrors);
        }

        public static DomainResult Error(IEnumerable<DomainError> domainErrors)
        {
            return new DomainResult(domainErrors);
        }

        public void EnsureSucces()
        {
            if (Failed) throw new DomainValidationException(DomainErrors);
        }
    }
}