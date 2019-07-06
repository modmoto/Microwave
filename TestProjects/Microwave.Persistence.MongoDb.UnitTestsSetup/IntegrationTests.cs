using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microwave.Persistence.MongoDb.UnitTestsSetup
{
    public class IntegrationTests
    {
        protected MicrowaveDatabase EventDatabase;

        [TestInitialize]
        public void SetupMongoDb()
        {
            var writeModelConfiguration = new MicrowaveConfiguration
            {
                DatabaseConfiguration = new DatabaseConfiguration
                {
                    DatabaseName = "IntegrationTest"
                }
            };

            EventDatabase = new MicrowaveDatabase(writeModelConfiguration);
            EventDatabase.Database.Client.DropDatabase("IntegrationTest");
        }
    }
}