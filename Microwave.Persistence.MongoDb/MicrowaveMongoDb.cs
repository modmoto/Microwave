using MongoDB.Driver;

namespace Microwave.Persistence.MongoDb
{
    public class MicrowaveMongoDb
    {
        private readonly string _databaseName;
        private readonly string _connectionString;
        public IMongoDatabase Database
        {
            get
            {
                var client = new MongoClient(_connectionString);
                return client.GetDatabase(_databaseName);
            }
        }

        public MicrowaveMongoDb(
            string databaseName = "MicrowaveDb",
            string connectionString = "mongodb://localhost:27017/")
        {
            _databaseName = databaseName;
            _connectionString = connectionString;
        }
    }
}