using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain;
using Microwave.EventStores;
using Microwave.Queries;

namespace Microwave.DependencyInjectionExtensions.UnitTests
{
    [TestClass]
    public class ConfigTests
    {
        [TestMethod]
        public void ConfigTest_ReadModel()
        {
            var confiNew = new ReadModelConfiguration(new Uri("http://localhost:5000/"))
            {
                ReadModelConfig = new ReadModelConfig()
                {
                    { typeof(TestReadModel), new Uri("http://localhost:6000/Api/DomainEvents")}
                }
            };
            Assert.AreEqual("http://localhost:6000/Api/DomainEvents", confiNew.GetReadModelLocation(typeof(TestReadModel)).ToString());
        }

        [TestMethod]
        public void ConfigTest_ReadModel_Default()
        {
            var confiNew = new ReadModelConfiguration(new Uri("http://localhost:5000/"));
            Assert.AreEqual("http://localhost:5000/", confiNew.GetReadModelLocation(typeof(TestReadModel)).ToString());
        }

        [TestMethod]
        public void ConfigTest_ReadModelDbConnection()
        {
            var confiNew = new ReadModelConfiguration(new Uri("http://localhost:5000/"))
            {
                Database = new Queries.DatabaseConfig
                {
                    DatabaseName = "OwnDbName",
                    ConnectionString = "Connection"
                }
            };

            Assert.AreEqual("OwnDbName", confiNew.Database.DatabaseName);
            Assert.AreEqual("Connection", confiNew.Database.ConnectionString);
        }

        [TestMethod]
        public void ConfigTest_ReadModelDbConnection_Default()
        {
            var confiNew = new ReadModelConfiguration(new Uri("http://localhost:5000/"));

            Assert.AreEqual("MicrowaveReadModelDb", confiNew.Database.DatabaseName);
            Assert.AreEqual("mongodb://localhost:27017/", confiNew.Database.ConnectionString);
        }
    }

    public class TestReadModel : ReadModel, IHandle<Ev>
    {
        public void Handle(Ev domainEvent)
        {
        }

        public override Type GetsCreatedOn { get; }
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