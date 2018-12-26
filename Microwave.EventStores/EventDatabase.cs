using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Microwave.EventStores
{
    public class EventDatabase
    {
        public IMongoDatabase Database { get; }
        public EventDatabase(IConfiguration config)
        {
            var writeModelDbSection = config.GetSection("WriteModelDatabase");
            var connectionString = writeModelDbSection["ConnectionString"] ?? "mongodb://localhost:27017/";
            var client = new MongoClient(connectionString);

            var dbName = writeModelDbSection["DatabaseName"] ?? "MicrowaveWriteModelDb";
            Database = client.GetDatabase(dbName);
        }
    }
}