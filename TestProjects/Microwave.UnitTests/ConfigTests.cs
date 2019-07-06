using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain.EventSourcing;
using Microwave.Domain.Identities;
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
                DatabaseConfiguration = new DatabaseConfiguration
                {
                    DatabaseName = "OwnDbName",
                    ConnectionString = "Connection"
                }
            };

            Assert.AreEqual("OwnDbName", confiNew.DatabaseConfiguration.DatabaseName);
            Assert.AreEqual("Connection", confiNew.DatabaseConfiguration.ConnectionString);
        }

        [TestMethod]
        public void ConfigTest_ReadModelDbConnection_Default()
        {
            var confiNew = new MicrowaveConfiguration();

            Assert.AreEqual("MicrowaveDb", confiNew.DatabaseConfiguration.DatabaseName);
            Assert.AreEqual("mongodb://localhost:27017/", confiNew.DatabaseConfiguration.ConnectionString);
        }
    }

    public class TestReadModel : ReadModel, IHandle<Ev>
    {
        public void Handle(Ev domainEvent)
        {
        }

        public override Type GetsCreatedOn => typeof(Ev);
    }

    public class Ev : IDomainEvent, ISubscribedDomainEvent
    {
        public Ev(Identity entityId)
        {
            EntityId = entityId;
        }

        public Identity EntityId { get; }
    }
}