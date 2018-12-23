using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;

namespace Microwave.Eventstores.UnitTests
{
    public class IntegrationTests
    {
        protected IMongoDatabase Database;

        [TestInitialize]
        public void SetupMongoDb()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            Database = client.GetDatabase("IntegrationTest");
            client.DropDatabase("IntegrationTest");
        }
    }
}