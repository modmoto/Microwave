using Microwave.Configuration.MongoDb;
using Microwave.Discovery;
using Microwave.EventStores;
using Microwave.Eventstores.Persistence.MongoDb;
using Microwave.EventStores.Ports;
using Microwave.Queries;
using Microwave.Queries.Persistence.MongoDb;

namespace Microwave.Persistence.UnitTests
{
    public class MongoDbTestSetup : IPersistenceDefinition
    {
        public MongoDbTestSetup()
        {
            var writeModelConfiguration = new MicrowaveConfiguration
            {
                DatabaseConfiguration = new DatabaseConfiguration
                {
                    DatabaseName = "IntegrationTest"
                }
            };

            EventDatabase = new MicrowaveDatabase(writeModelConfiguration);
            EventDatabase.Database.Client.DropDatabase("IntegrationTest");
        }

        public MicrowaveDatabase EventDatabase { get; }
        public IVersionRepository VersionRepository => new VersionRepository(EventDatabase);
        public IStatusRepository StatusRepository => new StatusRepository(EventDatabase, new EventLocationCache());
        public IReadModelRepository ReadModelRepository => new ReadModelRepository(EventDatabase);
        public ISnapShotRepository SnapShotRepository => new SnapShotRepository(EventDatabase);
        public IEventRepository EventRepository => new EventRepository(EventDatabase, new VersionCache(EventDatabase));
    }
}