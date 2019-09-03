using Microwave.Discovery;
using Microwave.Discovery.EventLocations;
using Microwave.Discovery.Subscriptions;
using Microwave.EventStores.Ports;
using Microwave.Persistence.MongoDb;
using Microwave.Persistence.MongoDb.Eventstores;
using Microwave.Persistence.MongoDb.Querries;
using Microwave.Queries;
using Microwave.Queries.Ports;

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

        public override IRemoteVersionReadRepository RemoteVersionReadRepository =>
            new RemoteVersionReadRepositoryMongoDb(EventMongoDb);
        public override IStatusRepository StatusRepository =>
            new StatusRepositoryMongoDb(EventMongoDb, new CacheThatNeverHasAnything());
        public override IReadModelRepository ReadModelRepository => new ReadModelRepositoryMongoDb(EventMongoDb);
        public override ISnapShotRepository SnapShotRepository => new SnapShotRepositoryMongoDb(EventMongoDb);
        public override IEventRepository EventRepository =>
            new EventRepositoryMongoDb(EventMongoDb, new VersionCache(EventMongoDb));

        public override ISubscriptionRepository SubscriptionRepository =>
            new SubscriptionRepositoryMongoDb(EventMongoDb, VersionRepository);
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