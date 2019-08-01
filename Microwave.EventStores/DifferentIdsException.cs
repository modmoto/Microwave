using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Microwave.Domain.Identities;

[assembly: InternalsVisibleTo("Microwave.Persistence.MongoDb")]
[assembly: InternalsVisibleTo("Microwave.Persistence.UnitTests")]
[assembly: InternalsVisibleTo("Microwave.Persistence.UnitTestSetupPorts")]
[assembly: InternalsVisibleTo("Microwave.Persistence.MongoDb.UnitTestsSetup")]
[assembly: InternalsVisibleTo("Microwave.Eventstores.UnitTests")]
[assembly: InternalsVisibleTo("Microwave")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace Microwave.EventStores
{
    [Serializable]
    public class DifferentIdsException : Exception
    {
        protected DifferentIdsException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }

        public DifferentIdsException(IEnumerable<Identity> identities)
            : base($"Not able to write to different streams in one turn, write them separatly: {string.Join(",", identities)} Ids to differe")
        {
        }
    }
}