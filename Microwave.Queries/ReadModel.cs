using System;
using System.Linq;
using Microwave.Domain.Identities;

namespace Microwave.Queries
{
    public abstract class ReadModel<T> : Query, IReadModel
    {
        public Type GetsCreatedOn => typeof(T);
        public long Version { get; set; }
        public Identity Identity { get; set; }
    }

    public interface IReadModel
    {
        Identity Identity { get; set; }
        Type GetsCreatedOn { get; }
        long Version { get; set; }
        void Handle(ISubscribedDomainEvent domainEvent, long version);
    }
}