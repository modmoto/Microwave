using System;
using System.Runtime.Serialization;

namespace Microwave
{
    [Serializable]
    public class IllegalDomainEventContructorException : Exception
    {
        protected IllegalDomainEventContructorException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
        public IllegalDomainEventContructorException(string message) : base(message)
        {
        }
    }
}