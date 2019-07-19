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

        public IEnumerable<DomainErrorRenamed> DomainErrors { get; }

        private DomainResult(IEnumerable<IDomainEvent> domainEvents)
        {
            _domainEvents = domainEvents;
            DomainErrors = new List<DomainErrorRenamed>();
        }

        private DomainResult(IEnumerable<DomainErrorRenamed> domainErrors)
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

        public static DomainResult Error(DomainErrorRenamed domainDomainErrorRenamed)
        {
            return new DomainResult(new List<DomainErrorRenamed> { domainDomainErrorRenamed });
        }

        public static DomainResult Error(string domainDomainErrorKey)
        {
            var domainError = new TypelessDomainErrorRenamed(domainDomainErrorKey);
            return new DomainResult(new List<DomainErrorRenamed> { domainError });
        }

        public static DomainResult Error(IEnumerable<string> domainDomainErrorKeys)
        {
            var enumDomainErrors = domainDomainErrorKeys.Select(e => new TypelessDomainErrorRenamed(e));
            return new DomainResult(enumDomainErrors);
        }

        public static DomainResult Error(Enum domainDomainErrorKey)
        {
            var domainError = new EnumDomainErrorRenamed(domainDomainErrorKey);
            return new DomainResult(new List<DomainErrorRenamed> { domainError });
        }

        public static DomainResult Error(IEnumerable<Enum> domainDomainErrorKeys)
        {
            var enumDomainErrors = domainDomainErrorKeys.Select(e => new EnumDomainErrorRenamed(e));
            return new DomainResult(enumDomainErrors);
        }

        public static DomainResult Error(IEnumerable<DomainErrorRenamed> domainErrors)
        {
            return new DomainResult(domainErrors);
        }

        public void EnsureSucces()
        {
            if (Failed) throw new DomainValidationException(DomainErrors);
        }
    }
}