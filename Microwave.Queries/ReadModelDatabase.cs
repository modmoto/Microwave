using MongoDB.Driver;

namespace Microwave.Queries
{
    public class ReadModelDatabase
    {
        public IMongoDatabase Database { get; }

        public ReadModelDatabase(IMongoDatabase database)
        {
            Database = database;
        }
    }
}