using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            var readModelConfiguration = new ReadModelConfiguration()
            {
                Database = new ReadDatabaseConfig
                {
                    DatabaseName = "IntegrationTest"
                }
            };

            var writeModelConfiguration = new WriteModelConfiguration()
            {
                Database = new WriteDatabaseConfig
                {
                    DatabaseName = "IntegrationTest"
                }
            };

            EventDatabase = new EventDatabase(writeModelConfiguration);
            EventDatabase.Database.Client.DropDatabase("IntegrationTest");
            ReadModelDatabase = new ReadModelDatabase(readModelConfiguration);
        }
    }
}