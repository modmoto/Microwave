using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microwave.Persistence.MongoDb.UnitTestsSetup
{
    public class IntegrationTests
    {
        protected MicrowaveMongoDb EventMongoDb;

        [TestInitialize]
        public void SetupMongoDb()
        {
            EventMongoDb = new MicrowaveMongoDb { DatabaseName = "MicrowaveIntegrationTest" };
            EventMongoDb.Database.Client.DropDatabase("MicrowaveIntegrationTest");
        }
    }
}