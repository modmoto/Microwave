using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security;
using Microwave.Discovery;
using Microwave.Discovery.EventLocations;
using Microwave.EventStores;
using Microwave.EventStores.Ports;
using Microwave.Persistence.CosmosDb;
using Microwave.Persistence.MongoDb.Eventstores;
using Microwave.Persistence.MongoDb.Querries;
using Microwave.Persistence.UnitTestSetupPorts;
using Microwave.Queries;
using Microwave.Queries.Ports;

namespace Microwave.Persistence.MongoDb.UnitTestsSetup
{
    public class CosmosDbTestSetup : IPersistenceLayerProvider
    {
        public CosmosDbTestSetup()
        {
            CosmosDb = new CosmosDb.CosmosDb();
            CosmosDb.PrimaryKey = PrimaryKey;
            CosmosDb.CosmosDbLocation = CosmosDbLocation;
            CosmosDb.InitializeCosmosDb();
           CosmosDbClient = new CosmosDbClient(CosmosDb, new List<Assembly>{ Assembly.GetCallingAssembly() });
           
        }

        public CosmosDbClient CosmosDbClient { get; }
        public CosmosDb.CosmosDb CosmosDb { get; }
        public IVersionRepository VersionRepository { get; }
        public IStatusRepository StatusRepository => new CosmosDbStatusRepository(CosmosDb);
        public IReadModelRepository ReadModelRepository => new CosmosDbReadModelRepository(CosmosDb);
        public ISnapShotRepository SnapShotRepository => new CosmosDbSnapshotRepository(CosmosDb);
        public IEventRepository EventRepository => new CosmosDbEventRepository(CosmosDbClient, CosmosDb, new List<Assembly> { Assembly.GetCallingAssembly() });

        public SecureString PrimaryKey
        {
            get
            {
                var secure = new SecureString();
                foreach (char c in "mCPtXM99gxlUalpz6bkFiWib2QD2OvIB9oEYj8tlpCPz1I4jSkOzlhJGnxAAEH4uiqWiYZ7enElzAM0lopKlJA==")
                {
                    secure.AppendChar(c);
                }

                return secure;
            }
        }

        public Uri CosmosDbLocation => new Uri("https://spoppinga.documents.azure.com:443/");
    }

}