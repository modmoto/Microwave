using System.Runtime.CompilerServices;
using Microwave.Discovery;
using Microwave.EventStores.Ports;
using Microwave.Queries;
using Microwave.Queries.Ports;

[assembly: InternalsVisibleTo("Microwave.Persistence.MongoDb.UnitTestsSetup")]
[assembly: InternalsVisibleTo("Microwave.Persistence.UnitTests")]
namespace Microwave.Persistence.UnitTestSetupPorts
{
    public abstract class PersistenceLayerProvider
    {
        public abstract IVersionRepository VersionRepository { get; }
        public abstract IStatusRepository StatusRepository { get; }
        public abstract IReadModelRepository ReadModelRepository { get; }
        public abstract ISnapShotRepository SnapShotRepository { get; }
        public abstract IEventRepository EventRepository { get; }
    }
}