using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Persistence.MongoDb;

namespace Microwave.Persistence.UnitTestsSetup.MongoDb
{
    public class IntegrationTests
    {
        protected MicrowaveMongoDb EventMongoDb;

        [TestInitialize]
        public void SetupMongoDb()
        {
            EventMongoDb = new MicrowaveMongoDb()
                .WithConnectionString(
                    "mongodb://157.90.1.251:3512/")
                .WithDatabaseName("MicrowaveIntegrationTest");

            EventMongoDb.Database.Client.DropDatabase("MicrowaveIntegrationTest");
        }
    }
}