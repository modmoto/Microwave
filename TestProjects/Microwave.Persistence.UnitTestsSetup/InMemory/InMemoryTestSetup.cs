using Microwave.Discovery;
using Microwave.EventStores.Ports;
using Microwave.Persistence.InMemory.Eventstores;
using Microwave.Persistence.InMemory.Querries;
using Microwave.Persistence.InMemory.Subscriptions;
using Microwave.Queries;
using Microwave.Queries.Ports;
using Microwave.Subscriptions.Ports;

namespace Microwave.Persistence.UnitTestsSetup.InMemory
{
    public class InMemoryTestSetup : PersistenceLayerProvider
    {
        public override IVersionRepository VersionRepository => new VersionRepositoryInMemory();
        public override IRemoteVersionReadModelRepository RemoteVersionReadModelRepository =>
            new RemoteVersionReadModelRepositoryInMemory(new SharedMemoryClass());
        public override IStatusRepository StatusRepository => new StatusRepositoryInMemory();
        public override IReadModelRepository ReadModelRepository => new ReadModelRepositoryInMemory();
        public override ISnapShotRepository SnapShotRepository => new SnapShotRepositoryInMemory();
        public override IEventRepository EventRepository => new EventRepositoryInMemory();
        public override ISubscriptionRepository SubscriptionRepository => new SubscriptionRepositoryInMemory();

        public override IRemoteVersionRepository RemoteVersionRepository =>
            new RemoteVersionRepositoryInMemory(VersionRepository, new SharedMemoryClass());
    }
}