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
                    "mongodb+srv://mongoDbTestUser:meinTestPw@cluster0-xhbcb.azure.mongodb.net/test?retryWrites=true&w=majority")
                .WithDatabaseName("MicrowaveIntegrationTest");

            EventMongoDb.Database.Client.DropDatabase("MicrowaveIntegrationTest");
        }
    }
}