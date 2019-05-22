using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain;

namespace Microwave.Persistence.MongoDb.UnitTests.Eventstores
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