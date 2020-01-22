namespace Microwave.Logging
{
    public enum MicrowaveLogLevel
    {
        Info = 2,
        Warning = 3,
        None = 5,
    }

    public class MicrowaveLogLevelType
    {
        public MicrowaveLogLevel LogLevel { get; }

        public MicrowaveLogLevelType(MicrowaveLogLevel logLevel)
        {
            LogLevel = logLevel;
        }
    }
}