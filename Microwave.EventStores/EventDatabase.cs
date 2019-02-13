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
        public WriteDatabaseConfig Database { get; set; } = new WriteDatabaseConfig();

    }

    public class WriteDatabaseConfig
    {
        public string ConnectionString { get; set; } = "mongodb://localhost:27017/";
        public string DatabaseName { get; set; } = "MicrowaveWriteModelDb";
    }
}