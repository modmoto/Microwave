namespace Application.Framework
{
    public struct ProcessedVersion
    {
        public long LastProcessedEventVersion { get; }
        public long StreamVersion { get; }

        public ProcessedVersion(long lastProcessedEventVersion, long streamVersion)
        {
            LastProcessedEventVersion = lastProcessedEventVersion;
            StreamVersion = streamVersion;
        }
    }
}