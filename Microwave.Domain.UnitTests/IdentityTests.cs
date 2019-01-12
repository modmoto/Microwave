using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microwave.Domain.UnitTests
{
    [TestClass]
    public class IdentityTests
    {
        [TestMethod]
        public void DoubleEqualsOperator()
        {
            var stringIdentity = StringIdentity.Create("jeah");
            var stringIdentity2 = StringIdentity.Create("jeah");

            Assert.IsTrue(stringIdentity == stringIdentity2);
            Assert.IsTrue(stringIdentity.Equals(stringIdentity2));
        }

        [TestMethod]
        public void DoubleEqualsOperator_Negative()
        {
            var stringIdentity = StringIdentity.Create("jeah");
            var stringIdentity2 = StringIdentity.Create("jeahNichtGleich");

            Assert.IsFalse(stringIdentity == stringIdentity2);
            Assert.IsFalse(stringIdentity.Equals(stringIdentity2));
        }

        [TestMethod]
        public void DoubleEqualsOperator_NegativeWithNull()
        {
            var stringIdentity = StringIdentity.Create("jeah");

            Assert.IsFalse(stringIdentity == null);
            Assert.IsFalse(stringIdentity.Equals(null));
        }

        [TestMethod]
        public void DoubleEqualsOperator_BothNull()
        {
            StringIdentity stringIdentity = null;
            Assert.IsTrue(stringIdentity == null);
        }

        [TestMethod]
        public void DoubleEqualsOperator_BothNull_NotEquals()
        {
            StringIdentity stringIdentity = null;
            Assert.IsFalse(stringIdentity != null);
        }

        [TestMethod]
        public void DoubleEqualsOperator_NegativeWithNull_Reveres()
        {
            var stringIdentity = StringIdentity.Create("jeah");

            Assert.IsFalse(null == stringIdentity);
        }
    }
}