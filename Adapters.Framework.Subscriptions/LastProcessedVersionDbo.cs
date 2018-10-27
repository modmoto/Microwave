using System.ComponentModel.DataAnnotations;

namespace Adapters.Framework.Subscriptions
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