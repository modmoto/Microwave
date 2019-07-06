using MongoDB.Driver;

namespace Microwave.Persistence.MongoDb
{
    public class MicrowaveMongoDb
    {
        public IMongoDatabase Database { get; }

        public MicrowaveMongoDb(
            string databaseName = "MicrowaveDb",
            string connectionString = "mongodb://localhost:27017/")
        {
            var client = new MongoClient(connectionString);
            Database = client.GetDatabase(databaseName);
        }
    }
}