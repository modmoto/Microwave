using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Microwave.Persistence.UnitTests")]
namespace Microwave.Queries
{
    public abstract class ReadModel<T> : ReadModelBase where T : ISubscribedDomainEvent
    {
        public override Type GetsCreatedOn => typeof(T);
        public override long Version { get; internal set; }
        public override string Identity { get; internal set; }
    }

    public abstract class ReadModelBase : Query
    {
        public abstract string Identity { get; internal set; }
        public abstract Type GetsCreatedOn { get; }
        public abstract long Version { get; internal set; }
    }
}