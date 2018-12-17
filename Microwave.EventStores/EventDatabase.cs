using MongoDB.Driver;

namespace Microwave.EventStores
{
    public class EventDatabase
    {
        public IMongoDatabase Database { get; }

        public EventDatabase(IMongoDatabase database)
        {
            Database = database;
        }
    }
}