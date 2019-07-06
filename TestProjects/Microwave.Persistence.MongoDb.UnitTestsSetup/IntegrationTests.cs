using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microwave.Persistence.MongoDb.UnitTestsSetup
{
    public class IntegrationTests
    {
        protected MicrowaveMongoDb EventMongoDb;

        [TestInitialize]
        public void SetupMongoDb()
        {
            EventMongoDb = new MicrowaveMongoDb("IntegrationTest");
            EventMongoDb.Database.Client.DropDatabase("IntegrationTest");
        }
    }
}