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
            var attribute = new UpdateEveryAttribute("* * * * *");
            var attributeNext = attribute.Next;

            Assert.AreEqual(attributeNext.Minute, DateTime.UtcNow.AddMinutes(1).Minute);
        }

        [TestMethod]
        public void ConstructorSeconds()
        {
            var attribute = new UpdateEveryAttribute(20);
            var attributeNext = attribute.Next;

            Assert.IsTrue(attributeNext.Second % 20 == 0);
        }

        [TestMethod]
        public void ConstructorSeconds_1()
        {
            var attribute = new UpdateEveryAttribute(1);
            var attributeNext = attribute.Next;

            Assert.IsTrue(attributeNext.Second % 1 == 0);
        }

        [TestMethod]
        public void ConstructorSeconds_0()
        {
            Assert.ThrowsException<InvalidTimeNotationException>(() => new UpdateEveryAttribute(0));
        }

        [TestMethod]
        [DataRow(1, 0, 1, 0)]
        [DataRow(10, 12, 20, 0)]
        [DataRow(15, 12, 15, 0)]
        [DataRow(15, 17, 30, 0)]
        [DataRow(15, 58, 0, 1)]
        [DataRow(5, 3, 5, 0)]
        public void AttributeCombinationTests(int secondsInput, int secondsForTest, int secondsOutput, int minuteOffset)
        {
            var updateEveryAttribute = new UpdateEveryAttribute(secondsInput, secondsForTest);
            var dateTime = updateEveryAttribute.Next;
            Assert.AreEqual(dateTime.Second, secondsOutput);
            Assert.AreEqual(dateTime.Minute, minuteOffset);
        }
    }
}