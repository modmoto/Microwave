using System;
using System.Linq;
using System.Runtime.Serialization;

namespace Microwave.Domain.Exceptions
{
    [Serializable]
    public class NotFoundException : Exception
    {
        protected NotFoundException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }

        public NotFoundException(Type type, string id) : base (CreateMessage(type, id))
        {
        }

        private static string CreateMessage(Type type, string id)
        {
            var typeName = type.Name;
            if (typeName == "ReadModelResult`1") typeName = type.GenericTypeArguments.First().Name;
            if (typeName == "EventStoreResult`1") typeName = type.GenericTypeArguments.First().Name;
            if (typeName == "IEnumerable`1")
            {
                var first = type.GenericTypeArguments.First();
                if (first.Name == "DomainEventWrapper")
                {
                    typeName = "DomainEvents";
                }
            }
            return $"Could not find {typeName} with ID {id}";
        }
    }
}