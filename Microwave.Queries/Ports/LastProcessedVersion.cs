using System;

namespace Microwave.Queries.Ports
{
    public class LastProcessedVersion
    {
        public LastProcessedVersion(string eventType, DateTimeOffset lastVersion)
        {
            EventType = eventType;
            LastVersion = lastVersion;
        }

        public string EventType { get; }
        public DateTimeOffset LastVersion { get; }
    }
}