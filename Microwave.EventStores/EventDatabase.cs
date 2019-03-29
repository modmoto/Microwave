using Microwave.Application;
using MongoDB.Driver;

namespace Microwave.EventStores
{
    public class EventDatabase
    {
        public IMongoDatabase Database { get; }
        public EventDatabase(MicrowaveConfiguration config)
        {
            var client = new MongoClient(config.ReadDatabase.ConnectionString);
            Database = client.GetDatabase(config.ReadDatabase.DatabaseName);
        }
    }
}