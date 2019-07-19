using System;

namespace Microwave.Domain.Identities
{
    public class GuidIdentity : Identity
    {
        public GuidIdentity(string id)
        {
            Id = id;
        }

        public new static GuidIdentity Create(Guid id)
        {
            return new GuidIdentity(id.ToString());
        }

        public static GuidIdentity Create()
        {
            return new GuidIdentity(Guid.NewGuid().ToString());
        }

        public Guid Guid => new Guid(Id);
    }
}