using System;
using System.Collections.Generic;

namespace Microwave.Domain
{
    public class DomainValidationException : Exception
    {
        public IEnumerable<DomainError> DomainErrors { get; }

        public DomainValidationException(IEnumerable<DomainError> domainErrors)
        {
            DomainErrors = domainErrors;
        }
    }
}