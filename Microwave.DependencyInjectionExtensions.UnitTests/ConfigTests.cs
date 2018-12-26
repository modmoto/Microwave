using System;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain;
using Microwave.Queries;

namespace Microwave.DependencyInjectionExtensions.UnitTests
{
    [TestClass]
    public class ConfigTests
    {
        [TestMethod]
        public void ConfigTest_ConfigNull()
        {
            Assert.ThrowsException<ArgumentException>(() => new EventLocationConfig(null));
        }

        [TestMethod]
        public void ConfigTest_BaseUrlNull()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings_valueForBaseUrlNull.test.json")
                .Build();
            var argumentException = Assert.ThrowsException<ArgumentException>(() => new EventLocationConfig(config));
            Assert.AreEqual("Baseurl for event feed not defined in appsettings.json.", argumentException.Message);
        }

        [TestMethod]
        public void ConfigTest_LocationDefNull()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings_valueForLocationsNull.test.json")
                .Build();
            var confiNew = new EventLocationConfig(config);
            Assert.AreEqual("http://localhost:5000/Api/DomainEventTypeStreams/whatever", confiNew.GetLocationForDomainEvent("whatever").ToString());
        }

        [TestMethod]
        public void ConfigTest_ReadModel()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test_ReadModelLocation.json")
                .Build();
            var confiNew = new EventLocationConfig(config);
            Assert.AreEqual("http://localhost:6000/Api/DomainEvents", confiNew.GetLocationForReadModel(typeof(TestReadModel).Name).ToString());
        }

        [TestMethod]
        public void ConfigTest_ReadModel_Default()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test_ReadModelLocation_Default.json")
                .Build();
            var confiNew = new EventLocationConfig(config);
            Assert.AreEqual("http://localhost:5000/Api/DomainEvents", confiNew.GetLocationForReadModel(typeof(TestReadModel).Name).ToString());
        }

        [TestMethod]
        public void ConfigTest_ReadModel_Default2()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test_ReadModelLocation_Default2.json")
                .Build();
            var confiNew = new EventLocationConfig(config);
            Assert.AreEqual("http://localhost:5000/Api/DomainEvents", confiNew.GetLocationForReadModel(typeof(TestReadModel).Name).ToString());
        }
    }

    public class TestReadModel : ReadModel, IHandle<Ev>
    {
        public void Handle(Ev domainEvent)
        {
        }
    }

    public class Ev : IDomainEvent
    {
        public Identity EntityId { get; }
    }
}