using System;
using System.Runtime.CompilerServices;
using Microwave.Domain.Identities;

[assembly: InternalsVisibleTo("Microwave.Persistence.UnitTests")]
namespace Microwave.Queries
{
    public abstract class ReadModel<T> : ReadModelBase
    {
        public override Type GetsCreatedOn => typeof(T);
        public override long Version { get; internal set; }
        public override Identity Identity { get; internal set; }
    }

    public abstract class ReadModelBase : Query
    {
        public abstract Identity Identity { get; internal set; }
        public abstract Type GetsCreatedOn { get; }
        public abstract long Version { get; internal set; }
    }
}