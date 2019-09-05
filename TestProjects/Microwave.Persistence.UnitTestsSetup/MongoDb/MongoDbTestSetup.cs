using Microwave.Discovery;
using Microwave.Discovery.EventLocations;
using Microwave.EventStores.Ports;
using Microwave.Persistence.MongoDb;
using Microwave.Persistence.MongoDb.Eventstores;
using Microwave.Persistence.MongoDb.Querries;
using Microwave.Persistence.MongoDb.Subscriptions;
using Microwave.Queries;
using Microwave.Queries.Ports;
using Microwave.Subscriptions;
using Microwave.Subscriptions.Ports;

namespace Microwave.Persistence.UnitTestsSetup.MongoDb
{
    public class MongoDbTestSetup : PersistenceLayerProvider
    {
        public MongoDbTestSetup()
        {
            EventMongoDb = new MicrowaveMongoDb()
                .WithConnectionString(
                    "mongodb+srv://mongoDbTestUser:meinTestPw@cluster0-xhbcb.azure.mongodb.net/test?retryWrites=true&w=majority")
                .WithDatabaseName("MicrowaveIntegrationTest");
            EventMongoDb.Database.Client.DropDatabase("MicrowaveIntegrationTest");
        }

        public MicrowaveMongoDb EventMongoDb { get; }
        public override IVersionRepository VersionRepository => new VersionRepositoryMongoDb(EventMongoDb);

        public override IRemoteVersionReadModelRepository RemoteVersionReadModelRepository =>
            new RemoteVersionReadModelRepositoryMongoDb(EventMongoDb);
        public override IStatusRepository StatusRepository =>
            new StatusRepositoryMongoDb(EventMongoDb, new CacheThatNeverHasAnything());
        public override IReadModelRepository ReadModelRepository => new ReadModelRepositoryMongoDb(EventMongoDb);
        public override ISnapShotRepository SnapShotRepository => new SnapShotRepositoryMongoDb(EventMongoDb);
        public override IEventRepository EventRepository =>
            new EventRepositoryMongoDb(EventMongoDb, new VersionCache(EventMongoDb));

        public override ISubscriptionRepository SubscriptionRepository =>
            new SubscriptionRepositoryMongoDb(EventMongoDb);

        public override IRemoteVersionRepository RemoteVersionRepository =>
            new RemoteVersionRepositoryMongoDb(EventMongoDb);
    }

    public class CacheThatNeverHasAnything : IEventLocationCache
    {
        public bool HasValue => false;
        public void Update(EventLocation eventLocation)
        {
        }

        public EventLocation GetValue()
        {
            return null;
        }
    }
}