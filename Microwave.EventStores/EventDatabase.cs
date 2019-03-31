using Microwave.Application;
using MongoDB.Driver;

namespace Microwave.EventStores
{
    public class EventDatabase
    {
        public IMongoDatabase Database { get; }
        public EventDatabase(MicrowaveConfiguration config)
        {
            var writeDatabase = config.WriteDatabase;
            var client = new MongoClient(writeDatabase.ConnectionString);
            Database = client.GetDatabase(writeDatabase.DatabaseName);
        }
    }
}