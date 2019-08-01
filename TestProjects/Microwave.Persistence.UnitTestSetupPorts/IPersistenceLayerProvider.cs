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
        internal abstract IVersionRepository VersionRepository { get; }
        internal abstract IStatusRepository StatusRepository { get; }
        internal abstract IReadModelRepository ReadModelRepository { get; }
        internal abstract ISnapShotRepository SnapShotRepository { get; }
        internal abstract IEventRepository EventRepository { get; }
    }
}