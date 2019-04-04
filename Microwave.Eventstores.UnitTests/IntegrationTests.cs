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
        protected MicrowaveDatabase MicrowaveDatabase;

        [TestInitialize]
        public void SetupMongoDb()
        {
            var writeModelConfiguration = new MicrowaveConfiguration
            {
                WriteDatabase = new WriteDatabaseConfig
                {
                    DatabaseName = "IntegrationTest"
                },
                ReadDatabase = new ReadDatabaseConfig
                {
                    DatabaseName = "IntegrationTest"
                }
            };

            EventDatabase = new MicrowaveDatabase(writeModelConfiguration);
            MicrowaveDatabase = new MicrowaveDatabase(writeModelConfiguration);
            EventDatabase.Database.Client.DropDatabase("IntegrationTest");
        }
    }
}