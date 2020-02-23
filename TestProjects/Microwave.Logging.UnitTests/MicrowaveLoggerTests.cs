using System;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microwave.Logging.UnitTests
{
    [TestClass]
    public class MicrowaveLoggerTests
    {
        [TestMethod]
        public void LogTrace_Null()
        {
            var microwaveLogger = new MicrowaveLogger<TestClassLog>();
            microwaveLogger.LogTrace("whatever");
        }

        [TestMethod]
        public void LogNull()
        {
            var microwaveLogger = CreateMicrowaveLogger(MicrowaveLogLevel.None);
            var message = "whatever";

            microwaveLogger.LogTrace(message);
            microwaveLogger.LogWarning(new Exception(), message);
        }

        [TestMethod]
        public void LogInformation()
        {
            var microwaveLogger = CreateMicrowaveLogger(MicrowaveLogLevel.Trace);
            var message = "whatever";

            microwaveLogger.LogTrace(message);
        }

        [TestMethod]
        public void LogWarning()
        {
            var microwaveLogger = CreateMicrowaveLogger(MicrowaveLogLevel.Warning);
            var message = "whatever";

            var exception = new Exception();
            microwaveLogger.LogWarning(exception, message);
        }

        private static MicrowaveLogger<TestClassLog> CreateMicrowaveLogger(MicrowaveLogLevel level)
        {
            var logger = new Mock<ILogger<TestClassLog>>();
            var microwaveLogger = new MicrowaveLogger<TestClassLog>(new MicrowaveLogLevelType(level), logger.Object);
            return microwaveLogger;
        }
    }

    public class TestClassLog
    {
    }
}