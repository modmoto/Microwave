using MongoDB.Driver;

namespace Microwave.Queries
{
    public class ReadModelDatabase
    {
        public IMongoDatabase Database { get; }
        public string ReadModelCollectionName { get; }
        public string QueryCollectionName { get; }
        public string LastProcessedVersionCollectionName { get; }

        public ReadModelDatabase(
            IMongoDatabase database,
            string readModelCollectionName = "ReadModelDbos",
            string queryCollectionName = "QueryDbos",
            string lastProcessedVersionCollectionName = "LastProcessedVersions")
        {
            Database = database;
            ReadModelCollectionName = readModelCollectionName;
            QueryCollectionName = queryCollectionName;
            LastProcessedVersionCollectionName = lastProcessedVersionCollectionName;
        }
    }
}