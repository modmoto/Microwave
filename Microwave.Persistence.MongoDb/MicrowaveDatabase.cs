using Microwave.Domain;
using MongoDB.Driver;

namespace Microwave.Persistence.MongoDb
{
    public class MicrowaveDatabase
    {
        public IMongoDatabase Database { get; }
        public MicrowaveDatabase(IMicrowaveConfiguration config)
        {
            var dbConfig = config.DatabaseConfiguration;
            var client = new MongoClient(dbConfig.ConnectionString);
            Database = client.GetDatabase(dbConfig.DatabaseName);
        }
    }
}