using System;
using Microwave.Domain.Identities;

namespace Microwave.Queries
{
    public abstract class ReadModel : Query
    {
        public abstract Type GetsCreatedOn { get; }
        public long Version { get; set; }
        public Identity Identity { get; set; }
    }
}