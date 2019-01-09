using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.WebApi.ApiFormatting.DateTimeOffset;
using Newtonsoft.Json;

namespace Microwave.WebApi.UnitTests
{
    [TestClass]
    public class ApiFormattingTests
    {
        [TestMethod]
        public  void DateTimeOffsetFormatting()
        {
            var identityConverter = new DateTimeOffsetConverter();
            var expected = "2019-01-07T22:06:35.3773970+01:00";
            var time = identityConverter.ReadJson(new Mockreader(expected), typeof(string), null, JsonSerializer
                .Create());
            Assert.AreEqual(DateTimeOffset.Parse("2019-01-07T22:06:35.3773970+01:00"), time);
        }

        [TestMethod]
        public  void DateTimeOffsetFormatting_NegOFfset()
        {
            var identityConverter = new DateTimeOffsetConverter();
            var expected = "2019-01-07T22:06:35.3773970-01:00";
            var time = identityConverter.ReadJson(new Mockreader(expected), typeof(string), null, JsonSerializer
                .Create());
            Assert.AreEqual(DateTimeOffset.Parse("2019-01-07T22:06:35.3773970-01:00"), time);
        }

        [TestMethod]
        public  void DateTimeOffsetFormatting_ApiError()
        {
            var identityConverter = new DateTimeOffsetConverter();
            var expected = "2019-01-07T22:06:35.3773970 01:00";
            var time = identityConverter.ReadJson(new Mockreader(expected), typeof(string), null, JsonSerializer
                .Create());
            Assert.AreEqual(DateTimeOffset.Parse("2019-01-07T22:06:35.3773970+01:00"), time);
        }

        [TestMethod]
        public  void DateTimeOffsetFormatting_NoString()
        {
            var identityConverter = new DateTimeOffsetConverter();
            var expected = 14;
            var time = identityConverter.ReadJson(new Mockreader(expected), typeof(string), null, JsonSerializer
                .Create());
            Assert.IsNull(time);
        }
    }

    public class Mockreader : JsonReader
    {
        private readonly object _value1;

        public Mockreader(object expectedValue)
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