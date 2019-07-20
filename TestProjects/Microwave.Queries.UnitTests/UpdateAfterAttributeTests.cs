using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microwave.Queries.UnitTests
{
    [TestClass]
    public class UpdateAfterAttributeTests
    {
        [TestMethod]
        public void Constructor()
        {
            var attribute = new UpdateAfterAttribute("* * * * *");
            var attributeNext = attribute.Next;

            Assert.AreEqual(attributeNext.Minute, DateTime.UtcNow.AddMinutes(1).Minute);
        }

        [TestMethod]
        public void ConstructorSeconds()
        {
            var attribute = new UpdateAfterAttribute(20);
            var attributeNext = attribute.Next;

            Assert.IsTrue(attributeNext.Second % 20 == 0);
        }

        [TestMethod]
        public void ConstructorSeconds_1()
        {
            var attribute = new UpdateAfterAttribute(1);
            var attributeNext = attribute.Next;

            Assert.IsTrue(attributeNext.Second % 1 == 0);
        }

        [TestMethod]
        public void ConstructorSeconds_0()
        {
            Assert.ThrowsException<InvalidTimeNotationException>(() => new UpdateAfterAttribute(0));
        }
    }
}