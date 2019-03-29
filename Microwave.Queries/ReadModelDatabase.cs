using Microwave.Application;
using MongoDB.Driver;

namespace Microwave.Queries
{
    public class ReadModelDatabase
    {
        public IMongoDatabase Database { get; }
        public ReadModelDatabase(MicrowaveConfiguration config)
        {
            var dbConfig = config.ReadDatabase;
            var client = new MongoClient(dbConfig.ConnectionString);
            Database = client.GetDatabase(dbConfig.DatabaseName);
        }
    }
}