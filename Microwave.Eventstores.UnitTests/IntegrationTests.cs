using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Application;
using Microwave.EventStores;
using Microwave.Persistence.MongoDb.Querries;
using Microwave.Queries;

namespace Microwave.Eventstores.UnitTests
{
    public class IntegrationTests
    {
        protected MicrowaveDatabase EventDatabase;

        [TestInitialize]
        public void SetupMongoDb()
        {
            var writeModelConfiguration = new MicrowaveConfiguration
            {
                DatabaseConfigDatabase = new DatabaseConfig
                {
                    DatabaseName = "IntegrationTest"
                }
            };

            EventDatabase = new MicrowaveDatabase(writeModelConfiguration);
            EventDatabase.Database.Client.DropDatabase("IntegrationTest");
        }
    }
}