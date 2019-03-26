using MongoDB.Driver;

namespace Microwave.Queries
{
    public class ReadModelDatabase
    {
        public IMongoDatabase Database { get; }
        public ReadModelDatabase(ReadModelConfiguration config)
        {
            var dbConfig = config.Database;
            var client = new MongoClient(dbConfig.ConnectionString);
            Database = client.GetDatabase(dbConfig.DatabaseName);
        }
    }

    public class ReadDatabaseConfig
    {
        public string ConnectionString { get; set; } = "mongodb://localhost:27017/";
        public string DatabaseName { get; set; } = "MicrowaveReadModelDb";
    }
}