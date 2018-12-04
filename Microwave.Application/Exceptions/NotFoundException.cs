using System;
using System.Runtime.Serialization;

namespace Microwave.Application.Exceptions
{
    [Serializable]
    public class NotFoundException : Exception
    {
        protected NotFoundException(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
        }

        public NotFoundException(Type type, string id) : base ($"Could not find entity {type.Name} with ID {id}")
        {
        }
    }
}