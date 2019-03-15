namespace Microwave.EventStores.Ports
{
    public class StreamVersion
    {
        public string DomainEventType { get; set; }
        public long Version { get; set; }
    }
}