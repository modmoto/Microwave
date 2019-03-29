using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Application;
using Microwave.EventStores;
using Microwave.Queries;

namespace Microwave.Eventstores.UnitTests
{
    public class IntegrationTests
    {
        protected EventDatabase EventDatabase;
        protected ReadModelDatabase ReadModelDatabase;

        [TestInitialize]
        public void SetupMongoDb()
        {
            var writeModelConfiguration = new MicrowaveConfiguration()
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

            EventDatabase = new EventDatabase(writeModelConfiguration);
            ReadModelDatabase = new ReadModelDatabase(writeModelConfiguration);
            EventDatabase.Database.Client.DropDatabase("IntegrationTest");
        }
    }
}