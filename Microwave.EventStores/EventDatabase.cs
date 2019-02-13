using MongoDB.Driver;

namespace Microwave.EventStores
{
    public class EventDatabase
    {
        public IMongoDatabase Database { get; }
        public EventDatabase(WriteModelConfiguration config)
        {
            var client = new MongoClient(config.Database.ConnectionString);
            Database = client.GetDatabase(config.Database.DatabaseName);
        }
    }

    public class WriteModelConfiguration
    {
        public DatabaseConfig Database { get; set; } = new DatabaseConfig();

    }

    public class DatabaseConfig
    {
        public string ConnectionString { get; set; } = "mongodb://localhost:27017/";
        public string DatabaseName { get; set; } = "MicrowaveWriteModelDb";
    }
}