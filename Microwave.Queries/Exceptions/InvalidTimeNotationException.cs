using System;
using System.Runtime.Serialization;

namespace Microwave.Queries.Exceptions
{
    [Serializable]
    internal class InvalidTimeNotationException : Exception
    {
        protected InvalidTimeNotationException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
        public InvalidTimeNotationException()
            : base("Choose a value between 1 and 60 seconds")
        {
        }
    }
}