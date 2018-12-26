using System;

namespace Microwave.Domain
{
    public interface IDomainEvent
    {
        Identity EntityId { get; }
    }

    public abstract class Identity : IEquatable<Identity>
    {
        public string Id { get; protected set; }

        public static bool operator== (Identity id1, Identity id2)
        {
            return (id1?.Id ?? string.Empty) == (id2?.Id ?? string.Empty);
        }

        public static bool operator!= (Identity id1, Identity id2)
        {
            return id1?.Equals(id2) == false;
        }

        public override int GetHashCode()
        {
            return Id != null ? Id.GetHashCode() : 0;
        }

        public static Identity Create(string entityId)
        {
            if (Guid.TryParse(entityId, out var guid))
                return GuidIdentity.Create(guid);
            return StringIdentity.Create(entityId);
        }

        public bool Equals(Identity other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Id, other.Id);
        }
    }

    public class StringIdentity : Identity
    {
        private StringIdentity(string id)
        {
            Id = id;
        }

        public new static StringIdentity Create(string id)
        {
            return new StringIdentity(id);
        }
    }

    public class GuidIdentity : Identity
    {
        private GuidIdentity(string id)
        {
            Id = id;
        }

        public static GuidIdentity Create(Guid id)
        {
            return new GuidIdentity(id.ToString());
        }
    }
}