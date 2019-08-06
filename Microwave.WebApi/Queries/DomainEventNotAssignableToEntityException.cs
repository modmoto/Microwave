using System;
using System.Runtime.Serialization;
using Microwave.Queries;

namespace Microwave.WebApi.Queries
{
    [Serializable]

    public class DomainEventNotAssignableToEntityException : Exception
    {
        protected DomainEventNotAssignableToEntityException(SerializationInfo serializationInfo, StreamingContext streamingContext)
                    : base(serializationInfo, streamingContext)
        {
        }

        public DomainEventNotAssignableToEntityException(ISubscribedDomainEvent domainevent)
            : base($"EntityId is null, can not assign {domainevent.GetType()}, most likely the event can not be parsed correctly")
        {
        }
    }
}