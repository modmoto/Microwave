using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microwave.Domain;

namespace Microwave.EventStores
{
    public class DifferentIdsException : Exception
    {
        protected DifferentIdsException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }

        public DifferentIdsException(IEnumerable<Identity> identities) : base($"Not able to write to different streams in one turn, write them separatly: {string.Join(",", identities)} Ids to differe")
        {
        }
    }
}