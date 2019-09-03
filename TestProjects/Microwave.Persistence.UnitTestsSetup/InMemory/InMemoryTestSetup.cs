using Microwave.Discovery;
using Microwave.Discovery.Subscriptions;
using Microwave.EventStores.Ports;
using Microwave.Persistence.InMemory.Eventstores;
using Microwave.Persistence.InMemory.Querries;
using Microwave.Queries;
using Microwave.Queries.Ports;

namespace Microwave.Persistence.UnitTestsSetup.InMemory
{
    public class InMemoryTestSetup : PersistenceLayerProvider
    {
        public override IVersionRepository VersionRepository => new VersionRepositoryInMemory();
        public override IRemoteVersionReadRepository RemoteVersionReadRepository =>
            new RemoteVersionReadRepositoryInMemory(new SharedMemoryClass());
        public override IStatusRepository StatusRepository => new StatusRepositoryInMemory();
        public override IReadModelRepository ReadModelRepository => new ReadModelRepositoryInMemory();
        public override ISnapShotRepository SnapShotRepository => new SnapShotRepositoryInMemory();
        public override IEventRepository EventRepository => new EventRepositoryInMemory();
        public override ISubscriptionRepository SubscriptionRepository =>
            new SubscriptionRepositoryInMemory(VersionRepository, new SharedMemoryClass());
    }
}