using Microwave.Discovery;
using Microwave.EventStores.Ports;
using Microwave.Queries;
using Microwave.Queries.Ports;

namespace Microwave.Persistence.UnitTestSetupPorts
{
    public interface IPersistenceLayerProvider
    {
        IVersionRepository VersionRepository { get; }
        IStatusRepository StatusRepository { get; }
        IReadModelRepository ReadModelRepository { get; }
        ISnapShotRepository SnapShotRepository { get; }
        IEventRepository EventRepository { get; }
    }
}