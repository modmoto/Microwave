using System;
using System.Runtime.Serialization;

namespace Microwave
{
    [Serializable]
    public class DuplicateDomainEventException : Exception
    {
        protected DuplicateDomainEventException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }

        public DuplicateDomainEventException(Type type)
            : base($"Not able to add ${type.Name} as a duplicate Event to a service, rename them or consider splitting the service")
        {
        }
    }
}