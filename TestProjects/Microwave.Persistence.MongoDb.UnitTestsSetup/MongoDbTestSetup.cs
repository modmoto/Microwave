using Microwave.Discovery;
using Microwave.Discovery.EventLocations;
using Microwave.EventStores.Ports;
using Microwave.Persistence.MongoDb.Eventstores;
using Microwave.Persistence.MongoDb.Querries;
using Microwave.Persistence.UnitTestSetupPorts;
using Microwave.Queries;
using Microwave.Queries.Ports;

namespace Microwave.Persistence.MongoDb.UnitTestsSetup
{
    public class MongoDbTestSetup : IPersistenceLayerProvider
    {
        public MongoDbTestSetup()
        {
            EventMongoDb = new MicrowaveMongoDb {
                ConnectionString = "mongodb+srv://mongoDbTestUser:meinTestPw@cluster0-xhbcb.azure.mongodb.net/test?retryWrites=true&w=majority",
                DatabaseName = "MicrowaveIntegrationTest" };
            EventMongoDb.Database.Client.DropDatabase("MicrowaveIntegrationTest");
        }

        public MicrowaveMongoDb EventMongoDb { get; }
        public IVersionRepository VersionRepository => new VersionRepository(EventMongoDb);
        public IStatusRepository StatusRepository => new StatusRepository(EventMongoDb, new CacheThatNeverHasAnything());
        public IReadModelRepository ReadModelRepository => new ReadModelRepository(EventMongoDb);
        public ISnapShotRepository SnapShotRepository => new SnapShotRepository(EventMongoDb);
        public IEventRepository EventRepository => new EventRepository(EventMongoDb, new VersionCache(EventMongoDb));
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