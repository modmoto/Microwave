using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Application;
using Microwave.Domain;
using Microwave.Queries;

namespace Microwave.DependencyInjectionExtensions.UnitTests
{
    [TestClass]
    public class ConfigTests
    {
        [TestMethod]
        public void ConfigTest_ReadModelDbConnection()
        {
            var confiNew = new MicrowaveConfiguration
            {
                DatabaseConfigDatabase = new DatabaseConfig
                {
                    DatabaseName = "OwnDbName",
                    ConnectionString = "Connection"
                }
            };

            Assert.AreEqual("OwnDbName", confiNew.DatabaseConfigDatabase.DatabaseName);
            Assert.AreEqual("Connection", confiNew.DatabaseConfigDatabase.ConnectionString);
        }

        [TestMethod]
        public void ConfigTest_ReadModelDbConnection_Default()
        {
            var confiNew = new MicrowaveConfiguration();

            Assert.AreEqual("MicrowaveReadModelDb", confiNew.DatabaseConfigDatabase.DatabaseName);
            Assert.AreEqual("mongodb://localhost:27017/", confiNew.DatabaseConfigDatabase.ConnectionString);
        }
    }

    public class TestReadModel : ReadModel, IHandle<Ev>
    {
        public void Handle(Ev domainEvent)
        {
        }

        public override Type GetsCreatedOn => typeof(Ev);
    }

    public class Ev : IDomainEvent
    {
        public Ev(Identity entityId)
        {
            EntityId = entityId;
        }

        public Identity EntityId { get; }
    }
}