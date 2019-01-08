using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain;
using Microwave.WebApi.ApiFormatting.Identity;
using Newtonsoft.Json;

namespace Microwave.WebApi.UnitTests
{
    [TestClass]
    public class ApiFormattingTests
    {
        [TestMethod]
        public  void IdentityFormatting()
        {
            var identityConverter = new IdentityConverter();
            var expected = "TestStringID";
            var identity = identityConverter.ReadJson(new Mockreader(expected), typeof(Identity), null, JsonSerializer
            .Create());
            Assert.AreEqual(StringIdentity.Create(expected), identity);
        }
    }

    public class Mockreader : JsonReader
    {
        private readonly object _value1;

        public Mockreader(string expectedValue)
        {
            _value1 = expectedValue;
        }

        public override object Value => _value1;

        public override bool Read()
        {
            return true;
        }
    }
}