using Microwave.Discovery;
using Microwave.EventStores;
using Microwave.EventStores.Ports;
using Microwave.Persistence.MongoDb.Eventstores;
using Microwave.Persistence.MongoDb.Querries;
using Microwave.Persistence.UnitTestSetupPorts;
using Microwave.Queries;

namespace Microwave.Persistence.MongoDb.UnitTestsSetup
{
    public class MongoDbTestSetup : IPersistenceDefinition
    {
        public MongoDbTestSetup()
        {
            EventMongoDb = new MicrowaveMongoDb("IntegrationTest");
            EventMongoDb.Database.Client.DropDatabase("IntegrationTest");
        }

        public MicrowaveMongoDb EventMongoDb { get; }
        public IVersionRepository VersionRepository => new VersionRepository(EventMongoDb);
        public IStatusRepository StatusRepository => new StatusRepository(EventMongoDb, new EventLocationCache());
        public IReadModelRepository ReadModelRepository => new ReadModelRepository(EventMongoDb);
        public ISnapShotRepository SnapShotRepository => new SnapShotRepository(EventMongoDb);
        public IEventRepository EventRepository => new EventRepository(EventMongoDb, new VersionCache(EventMongoDb));
    }
}