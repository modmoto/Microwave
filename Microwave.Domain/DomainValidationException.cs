using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microwave.Domain
{
    [Serializable]
    public class DomainValidationException : Exception
    {
        protected DomainValidationException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }

        public IEnumerable<DomainError> DomainErrors { get; }

        public DomainValidationException(IEnumerable<DomainError> domainErrors)
        {
            DomainErrors = domainErrors;
        }
    }
}