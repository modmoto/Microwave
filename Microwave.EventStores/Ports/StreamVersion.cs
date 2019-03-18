namespace Microwave.EventStores.Ports
{
    public class StreamVersion
    {
        public StreamVersion(string domainEventType, long version)
        {
            DomainEventType = domainEventType;
            Version = version;
        }

        public string DomainEventType { get; }
        public long Version { get; }
    }
}