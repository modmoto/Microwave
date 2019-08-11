using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain.Identities;
using Microwave.Domain.Validation;
using Microwave.Queries;
using Microwave.WebApi.ApiFormatting.DateTimeOffsets;
using Microwave.WebApi.ApiFormatting.Identities;
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

        [TestMethod]
        public  void DateTimeOffsetFormatting()
        {
            var identityConverter = new DateTimeOffsetConverter();
            var expected = "2019-01-07T22:06:35.3773970+01:00";
            var time = identityConverter.ReadJson(new Mockreader(expected), typeof(Identity), null, JsonSerializer
                .Create());
            Assert.AreEqual(DateTimeOffset.Parse("2019-01-07T22:06:35.3773970+01:00"), time);
        }

        [TestMethod]
        public  void DateTimeOffsetFormatting_NegOFfset()
        {
            var identityConverter = new DateTimeOffsetConverter();
            var expected = "2019-01-07T22:06:35.3773970-01:00";
            var time = identityConverter.ReadJson(new Mockreader(expected),
                typeof(Identity),
                null,
                JsonSerializer.Create());
            Assert.AreEqual(DateTimeOffset.Parse("2019-01-07T22:06:35.3773970-01:00"), time);
        }

        [TestMethod]
        public  void DateTimeOffsetFormatting_ApiError()
        {
            var identityConverter = new DateTimeOffsetConverter();
            var expected = "2019-01-07T22:06:35.3773970 01:00";
            var time = identityConverter.ReadJson(new Mockreader(expected), typeof(Identity), null, JsonSerializer
                .Create());
            Assert.AreEqual(DateTimeOffset.Parse("2019-01-07T22:06:35.3773970+01:00"), time);
        }

        [TestMethod]
        public  void DateTimeOffsetFormatting_NoString()
        {
            var identityConverter = new DateTimeOffsetConverter();
            var expected = 14;
            var time = identityConverter.ReadJson(new Mockreader(expected), typeof(Identity), null, JsonSerializer
                .Create());
            Assert.IsNull(time);
        }
    }

    public class TestError : DomainError
    {
        public TestError(string desc = null) : base(desc)
        {
        }
    }

    public class JsonTextWriterMock : JsonWriter
    {
        public override void Flush()
        {
        }

        public override void WriteRawValue(string value)
        {
            StringValue = value;
        }

        public override void WriteValue(object value)
        {
            Value = value;
        }

        public object Value { get; private set; }
        public string StringValue { get; private set; }
    }

    public class ReadModelTest : ReadModel<Mockreader>
    {
        public Identity IdentityField { get; set; }
        public string TestProp { get; set; }
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