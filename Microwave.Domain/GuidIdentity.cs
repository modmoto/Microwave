using System;

namespace Microwave.Domain
{
    public class GuidIdentity : Identity
    {
        private GuidIdentity(string id)
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