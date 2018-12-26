using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Microwave.Queries
{
    public class ReadModelDatabase
    {
        public IMongoDatabase Database { get; }
        public ReadModelDatabase(IConfiguration config)
        {
            var writeModelDbSection = config.GetSection("ReadModelDatabase");
            var connectionString = writeModelDbSection["ConnectionString"] ?? "mongodb://localhost:27017/";
            var client = new MongoClient(connectionString);

            var dbName = writeModelDbSection["DatabaseName"] ?? "MicrowaveReadModelDb";
            Database = client.GetDatabase(dbName);
        }
    }
}