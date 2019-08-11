using System;
using System.Linq;
using Microwave.Domain.Identities;

namespace Microwave.Queries
{
    public abstract class ReadModel<T> : Query, IReadModel
    {
        public long Version { get; set; }
        public Identity Identity { get; set; }
    }

    public interface IReadModel
    {
        Identity Identity { get; set; }
        long Version { get; set; }
        void Handle(ISubscribedDomainEvent domainEvent, long version);
    }

    public static class ReadModelExtensions
    {
        public static Type GetsCreatedOn(this IReadModel readModel)
        {
            var type = readModel.GetType();
            var genericTypeArguments = type.BaseType?.GenericTypeArguments;
            return genericTypeArguments?.First();
        }

    }
}