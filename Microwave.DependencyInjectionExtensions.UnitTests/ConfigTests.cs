using System;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Microwave.DependencyInjectionExtensions.UnitTests
{
    [TestFixture]
    public class ConfigTests
    {
        [Test]
        public void ConfigTest_ConfigNull()
        {
            Assert.Throws<ArgumentException>(() => new EventLocationConfig(null));
        }

        [Test]
        public void ConfigTest_BaseUrlNull()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings_valueForBaseUrlNull.test.json")
                .Build();
            var argumentException = Assert.Throws<ArgumentException>(() => new EventLocationConfig(config));
            Assert.AreEqual("Baseurl for event feed not defined in appsettings.json.", argumentException.Message);
        }

        [Test]
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