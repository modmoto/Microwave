using MongoDB.Driver;

namespace Microwave.Persistence.MongoDb
{
    public class MicrowaveMongoDb
    {
        public string DatabaseName { get; private set; } = "MicrowaveDb";
        public string ConnectionString { get; private set; } = "mongodb://localhost:27017/";

        public IMongoDatabase Database
        {
            get
            {
                var client = new MongoClient(ConnectionString);
                return client.GetDatabase(DatabaseName);
            }
        }

        public MicrowaveMongoDb WithDatabaseName(string databaseName)
        {
            DatabaseName = databaseName;
            return this;
        }

        public MicrowaveMongoDb WithConnectionString(string connectionString)
        {
            ConnectionString = connectionString;
            return this;
        }
    }
}