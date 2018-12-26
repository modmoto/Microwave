using MongoDB.Driver;

namespace Microwave.EventStores
{
    public class EventDatabase
    {
        public IMongoDatabase Database { get; }
        public string EventCollectionName { get; }
        public string SnapshotCollectionName { get; }

        public EventDatabase(
            IMongoDatabase database,
            string eventCollectionName = "DomainEventDbos",
            string snapshotCollectionName = "SnapshotDbos")
        {
            Database = database;
            EventCollectionName = eventCollectionName;
            SnapshotCollectionName = snapshotCollectionName;
        }
    }
}