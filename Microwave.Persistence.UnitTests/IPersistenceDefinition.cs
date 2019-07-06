using Microwave.Discovery;
using Microwave.EventStores;
using Microwave.EventStores.Ports;
using Microwave.Queries;

namespace Microwave.Persistence.UnitTests
{
    public interface IPersistenceDefinition
    {
        IVersionRepository VersionRepository { get; }
        IStatusRepository StatusRepository { get; }
        IReadModelRepository ReadModelRepository { get; }
        ISnapShotRepository SnapShotRepository { get; }
        IEventRepository EventRepository { get; }
    }
}