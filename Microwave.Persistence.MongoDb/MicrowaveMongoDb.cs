using MongoDB.Driver;

namespace Microwave.Persistence.MongoDb
{
    public class MicrowaveMongoDb
    {
        public string DatabaseName { get; set; } = "MicrowaveDb";
        public string ConnectionString { get; set; } = "mongodb://localhost:27017/";

        public IMongoDatabase Database
        {
            get
            {
                var client = new MongoClient(ConnectionString);
                return client.GetDatabase(DatabaseName);
            }
        }
    }
}