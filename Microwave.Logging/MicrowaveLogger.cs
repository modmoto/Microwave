using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microwave.Logging
{
    public class MicrowaveLogger<T> : IMicrowaveLogger<T>
    {
        private readonly ILogger<T> _logger;
        private readonly MicrowaveLogLevel _logLevel;

        public MicrowaveLogger(
            MicrowaveLogLevelType logLevel,
            ILogger<T> logger)
        {
            _logLevel = logLevel.LogLevel;
            _logger = logger;
        }

        public MicrowaveLogger()
        {
            _logLevel = MicrowaveLogLevel.None;
            _logger = new Logger<T>(new NullLoggerFactory());
        }

        public void LogInformation(string message)
        {
            if (_logLevel <= MicrowaveLogLevel.Info)
            {
                Console.WriteLine($"Microwave: {message}");
            }
        }

        public void LogWarning(Exception exception, string message)
        {
            if (_logLevel <= MicrowaveLogLevel.Warning)
            {
                _logger.LogWarning(exception, $"Microwave: {message}");
            }
        }
    }
}