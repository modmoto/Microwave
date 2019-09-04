using System;

namespace Microwave.Subscriptions
{
    public class RemoteVersion
    {
        public RemoteVersion(string eventType, DateTimeOffset lastVersion)
        {
            EventType = eventType;
            LastVersion = lastVersion;
        }

        public string EventType { get; }
        public DateTimeOffset LastVersion { get; }

        protected bool Equals(RemoteVersion other)
        {
            return EventType == other.EventType && LastVersion.Equals(other.LastVersion);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((RemoteVersion) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((EventType != null ? EventType.GetHashCode() : 0) * 397) ^ LastVersion.GetHashCode();
            }
        }
    }
}