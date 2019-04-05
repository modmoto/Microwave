using System;

namespace Microwave.EventStores.Ports
{
    public class StreamVersion
    {
        public StreamVersion(string domainEventType, DateTimeOffset lastOccured)
        {
            DomainEventType = domainEventType;
            LastOccured = lastOccured;
        }

        public string DomainEventType { get; }
        public DateTimeOffset LastOccured { get; }
    }
}