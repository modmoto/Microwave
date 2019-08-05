using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microwave.Persistence.MongoDb.UnitTestsSetup
{
    public class IntegrationTests
    {
        protected MicrowaveMongoDb EventMongoDb;

        [TestInitialize]
        public void SetupMongoDb()
        {
            EventMongoDb = new MicrowaveMongoDb
            {
                ConnectionString = "mongodb+srv://mongoDbTestUser:meinTestPw@cluster0-xhbcb.azure.mongodb.net/test?retryWrites=true&w=majority",
                DatabaseName = "MicrowaveIntegrationTest"
            };

            EventMongoDb.Database.Client.DropDatabase("MicrowaveIntegrationTest");
        }
    }
}