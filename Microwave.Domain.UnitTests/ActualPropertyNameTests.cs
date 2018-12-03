using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microwave.Domain.UnitTests
{
    [TestClass]
    public class ActualPropertyNameTests
    {
        [TestMethod]
        public void Constructor_SinglePath()
        {
            var actualPropertyName = new ActualPropertyName("Name");
            Assert.AreEqual("Name", actualPropertyName.Path[0]);

        }

        [TestMethod]
        public void Constructor_MultiPath()
        {
            var actualPropertyName = new ActualPropertyName("User.Name");
            Assert.AreEqual("User", actualPropertyName.Path[0]);
            Assert.AreEqual("Name", actualPropertyName.Path[1]);
        }

        [TestMethod]
        public void Constructor_Exception()
        {
            Assert.ThrowsException<ArgumentException>(() => new ActualPropertyName(".User.Name"));
        }
    }
}