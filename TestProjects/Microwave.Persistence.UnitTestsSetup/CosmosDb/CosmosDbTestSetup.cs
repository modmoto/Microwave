using System;
using System.Security;
using Microwave.Discovery;
using Microwave.EventStores.Ports;
using Microwave.Persistence.CosmosDb;
using Microwave.Queries;
using Microwave.Queries.Ports;

namespace Microwave.Persistence.UnitTestsSetup.CosmosDb
{
    public class CosmosDbTestSetup : PersistenceLayerProvider
    {
        public CosmosDbTestSetup()
        {
            CosmosDb = new Persistence.CosmosDb.CosmosDb();
            CosmosDb.PrimaryKey = PrimaryKey;
            CosmosDb.CosmosDbLocation = CosmosDbLocation;
            CosmosDb.InitializeCosmosDb().Wait();
        }

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

        public Persistence.CosmosDb.CosmosDb CosmosDb { get; }
        public override IVersionRepository VersionRepository => new CosmosDbVersionRepository(CosmosDb);
        public override IStatusRepository StatusRepository => new CosmosDbStatusRepository(CosmosDb);
        public override IReadModelRepository ReadModelRepository => new CosmosDbReadModelRepository(CosmosDb);
        public override ISnapShotRepository SnapShotRepository => new CosmosDbSnapshotRepository(CosmosDb);
        public override IEventRepository EventRepository => new CosmosDbEventRepository(CosmosDb, new AssemblyProvider(), new CosmosDbVersionCache(CosmosDb));
    }

}