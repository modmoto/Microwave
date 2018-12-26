using System;

namespace Microwave.Domain
{
    public interface IDomainEvent
    {
        Identity EntityId { get; }
    }

    public abstract class Identity
    {
        public string Id { get; protected set; }

        public bool Equals(Identity other)
        {
            return string.Equals(Id, other.Id);
        }

        public static Identity Create(string entityId)
        {
            if (Guid.TryParse(entityId, out var guid))
                return GuidIdentity.Create(guid);
            return StringIdentity.Create(entityId);
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