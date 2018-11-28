using System.ComponentModel.DataAnnotations;

namespace Microwave.Subscriptions
{
    public class LastProcessedVersionDbo
    {
        public LastProcessedVersionDbo(string eventType, long lastVersion)
        {
            EventType = eventType;
            LastVersion = lastVersion;
        }

        [Key] public string EventType { get; set; }

        public long LastVersion { get; set; }
    }
}