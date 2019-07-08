using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microwave.Persistence.MongoDb.UnitTestsSetup
{
    public class IntegrationTests
    {
        protected MicrowaveMongoDb EventMongoDb;

        [TestInitialize]
        public void SetupMongoDb()
        {
            EventMongoDb = new MicrowaveMongoDb { DatabaseName = "IntegrationTest" };
            EventMongoDb.Database.Client.DropDatabase("IntegrationTest");
        }
    }
}