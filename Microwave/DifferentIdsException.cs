using System;
using System.Runtime.Serialization;

namespace Microwave
{
    [Serializable]
    internal class DuplicateDomainEventException : Exception
    {
        protected DuplicateDomainEventException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }

        public DuplicateDomainEventException(string domainEventName) : base($"Not able to add ${domainEventName} as a duplicate Event to a service, rename them or consider splitting the service")
        {
        }
    }
}