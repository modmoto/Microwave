using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.EventStores;
using Microwave.Queries;
using MongoDB.Driver;

namespace Microwave.Eventstores.UnitTests
{
    public class IntegrationTests
    {
        protected EventDatabase EventDatabase;
        protected ReadModelDatabase ReadModelDatabase;

        [TestInitialize]
        public void SetupMongoDb()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json")
                .Build();

            var client = new MongoClient("mongodb://localhost:27017");
            EventDatabase = new EventDatabase(config);
            EventDatabase.Database.Client.DropDatabase("IntegrationTest");
            ReadModelDatabase = new ReadModelDatabase(config);
        }
    }
}