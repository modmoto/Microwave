using System.Runtime.CompilerServices;
using MongoDB.Driver;

[assembly: InternalsVisibleTo("Microwave.Persistence.MongoDb.UnitTestsSetup")]
[assembly: InternalsVisibleTo("Microwave.Queries.UnitTests")]
[assembly: InternalsVisibleTo("Microwave.Eventstores.UnitTests")]
[assembly: InternalsVisibleTo("Microwave.Discovery.UnitTests")]
namespace Microwave.Persistence.MongoDb
{
    public class MicrowaveMongoDb
    {
        public MicrowaveMongoDb()
        {
        }

        public string DatabaseName = "MicrowaveDb";
        public string ConnectionString = "mongodb://localhost:27017/";

        public IMongoDatabase Database
        {
            get
            {
                var client = new MongoClient(ConnectionString);
                return client.GetDatabase(DatabaseName);
            }
        }

        public void WithDatabaseName(string databaseName)
        {
            DatabaseName = databaseName;
        }

        public void WithConnectionString(string connectionString)
        {
            ConnectionString = connectionString;
        }
    }
}