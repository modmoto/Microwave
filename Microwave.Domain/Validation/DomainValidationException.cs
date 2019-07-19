using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microwave.Domain.Validation
{
    [Serializable]
    public class DomainValidationException : Exception
    {
        protected DomainValidationException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }

        public IEnumerable<DomainErrorRenamed> DomainErrors { get; }

        public DomainValidationException(IEnumerable<DomainErrorRenamed> domainErrors)
        {
            DomainErrors = domainErrors;
        }
    }
}