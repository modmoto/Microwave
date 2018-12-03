using System;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            Assert.AreEqual("http://localhost:5000/Api/DomainEventTypeStreams/whatever", confiNew.GetLocationFor("whatever").ToString());
        }
    }
}