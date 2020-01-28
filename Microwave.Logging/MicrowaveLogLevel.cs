namespace Microwave.Logging
{
    public enum MicrowaveLogLevel
    {
        Trace = 1,
        Info = 2,
        Warning = 3,
        None = 5
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