using Microwave.Discovery;
using Microwave.EventStores.Ports;
using Microwave.Persistence.InMemory.Eventstores;
using Microwave.Persistence.InMemory.Querries;
using Microwave.Persistence.UnitTestSetupPorts;
using Microwave.Queries;
using Microwave.Queries.Ports;

namespace Microwave.Persistence.MongoDb.UnitTestsSetup
{
    public class InMemroyTestSetup : PersistenceLayerProvider
    {
        public override IVersionRepository VersionRepository => new VersionRepositoryInMemory();
        public override IStatusRepository StatusRepository => new StatusRepositoryInMemory();
        public override IReadModelRepository ReadModelRepository => new ReadModelRepositoryInMemory();
        public override ISnapShotRepository SnapShotRepository => new SnapShotRepositoryInMemory();
        public override IEventRepository EventRepository => new EventRepositoryInMemory();
    }
}