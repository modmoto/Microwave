using MongoDB.Driver;

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