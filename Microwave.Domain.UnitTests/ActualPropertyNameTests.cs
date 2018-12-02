using System;
using NUnit.Framework;

namespace Microwave.Domain.UnitTests
{
    public class ActualPropertyNameTests
    {
        [Test]
        public void Constructor_SinglePath()
        {
            var actualPropertyName = new ActualPropertyName("Name");
            Assert.AreEqual("Name", actualPropertyName.Path[0]);

        }

        [Test]
        public void Constructor_MultiPath()
        {
            var actualPropertyName = new ActualPropertyName("User.Name");
            Assert.AreEqual("User", actualPropertyName.Path[0]);
            Assert.AreEqual("Name", actualPropertyName.Path[1]);
        }

        [Test]
        public void Constructor_Exception()
        {
            Assert.Throws<ArgumentException>(() => new ActualPropertyName(".User.Name"));
        }
    }
}