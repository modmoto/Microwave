using System;
using System.Runtime.Serialization;

namespace Microwave.WebApi
{
    
    [Serializable]
    public class InvalidReadModelCreationTypeException : Exception
    {
        protected InvalidReadModelCreationTypeException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
        
        public InvalidReadModelCreationTypeException(string readModel) : base(
            $"Can not instantiate Readmodel {readModel} as it is missing a valid creationtype")
        {
        }
    }
}