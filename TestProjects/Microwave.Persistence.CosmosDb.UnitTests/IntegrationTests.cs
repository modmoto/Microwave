using System;
using System.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microwave.Persistence.CosmosDb.UnitTests
{
    public class IntegrationTests
    {
        protected CosmosDb Database;

        public SecureString PrimaryKey
        {
            get
            {
                var secure = new SecureString();
                foreach (char c in "mCPtXM99gxlUalpz6bkFiWib2QD2OvIB9oEYj8tlpCPz1I4jSkOzlhJGnxAAEH4uiqWiYZ7enElzAM0lopKlJA==")
                {
                    secure.AppendChar(c);
                }

                return secure;
            }
        }

        public Uri CosmosDbLocation => new Uri("https://spoppinga.documents.azure.com:443/");

        [TestInitialize]
        public void SetupMongoDb()
        {
            Database = new CosmosDb
            {
                PrimaryKey = PrimaryKey,
                CosmosDbLocation = CosmosDbLocation
            };
        }
    }
}