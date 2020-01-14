using System;
using System.Runtime.Serialization;

namespace Microwave
{
    [Serializable]

    public class ProvidedTypeIsNoEventFeedException : Exception
    {
        protected ProvidedTypeIsNoEventFeedException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }

        public ProvidedTypeIsNoEventFeedException(Type type) : base(
            $"Type: {type.Name} does not implement a IEventFeed<T> and therefore can not be used")
        {
        }
    }
}