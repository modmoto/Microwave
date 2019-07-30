using System.Runtime.CompilerServices;
using Microwave.Discovery;
using Microwave.Discovery.EventLocations;
using Microwave.EventStores.Ports;
using Microwave.Persistence.MongoDb.Eventstores;
using Microwave.Persistence.MongoDb.Querries;
using Microwave.Persistence.UnitTestSetupPorts;
using Microwave.Queries;
using Microwave.Queries.Ports;

[assembly: InternalsVisibleTo("Microwave.Persistence.UnitTests")]
namespace Microwave.Persistence.MongoDb.UnitTestsSetup
{
    internal class MongoDbTestSetup : PersistenceLayerProvider
    {
        public MongoDbTestSetup()
        {
            EventMongoDb = new MicrowaveMongoDb {
                ConnectionString = "mongodb+srv://mongoDbTestUser:meinTestPw@cluster0-xhbcb.azure.mongodb.net/test?retryWrites=true&w=majority",
                DatabaseName = "MicrowaveIntegrationTest" };
            EventMongoDb.Database.Client.DropDatabase("MicrowaveIntegrationTest");
        }

        public MicrowaveMongoDb EventMongoDb { get; }
        internal override IVersionRepository VersionRepository => new VersionRepository(EventMongoDb);
        internal override IStatusRepository StatusRepository => new StatusRepository(EventMongoDb, new CacheThatNeverHasAnything());
        internal override IReadModelRepository ReadModelRepository => new ReadModelRepository(EventMongoDb);
        internal override ISnapShotRepository SnapShotRepository => new SnapShotRepository(EventMongoDb);
        internal override IEventRepository EventRepository => new EventRepository(EventMongoDb, new VersionCache
        (EventMongoDb));
    }

    internal class CacheThatNeverHasAnything : IEventLocationCache
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