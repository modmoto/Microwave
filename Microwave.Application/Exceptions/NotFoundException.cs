using System;
using System.Linq;
using System.Runtime.Serialization;

namespace Microwave.Application.Exceptions
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
            if (typeName == "ReadModelWrapper`1") typeName = type.GenericTypeArguments.First().Name;
            return $"Could not find entity {typeName} with ID {id}";
        }
    }
}