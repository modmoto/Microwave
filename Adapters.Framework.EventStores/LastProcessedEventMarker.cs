using System;

namespace Adapters.Framework.EventStores
{
    public class LastProcessedEventMarker
    {
        public LastProcessedEventMarker(
            long lastProcessedVersion,
            Guid processedEventEntityId,
            Guid processedDomainEventId)
        {
            LastProcessedVersion = lastProcessedVersion;
            ProcessedEventEntityId = processedEventEntityId;
            ProcessedDomainEventId = processedDomainEventId;
        }

        public long LastProcessedVersion { get; }
        public Guid ProcessedEventEntityId { get; }
        public Guid ProcessedDomainEventId { get; }
    }
}