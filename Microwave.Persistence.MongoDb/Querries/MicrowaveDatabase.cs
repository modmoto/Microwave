using Microwave.Application;
using MongoDB.Driver;

namespace Microwave.Persistence.MongoDb.Querries
{
    public class MicrowaveDatabase
    {
        public IMongoDatabase Database { get; }
        public MicrowaveDatabase(MicrowaveConfiguration config)
        {
            var dbConfig = config.DatabaseConfiguration;
            var client = new MongoClient(dbConfig.ConnectionString);
            Database = client.GetDatabase(dbConfig.DatabaseName);
        }
    }
}