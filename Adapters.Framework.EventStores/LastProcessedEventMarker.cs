using System;

namespace Adapters.Framework.EventStores
{
    public class LastProcessedEventMarker
    {
        public LastProcessedEventMarker(
            long lastProcessedVersion,
            Guid processedDomainEventId)
        {
            LastProcessedVersion = lastProcessedVersion;
            ProcessedDomainEventId = processedDomainEventId;
        }

        public long LastProcessedVersion { get; }
        public Guid ProcessedDomainEventId { get; }
    }
}