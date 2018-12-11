using System;

namespace Microwave.Domain
{
    public interface IDomainEvent
    {
        Guid EntityId { get; }
    }

    public interface IIdentifiable
    {
        bool IsSameId(object other);
    }

    public class Identifier<T> : IIdentifiable
    {
        public T Id { get; }

        public Identifier(T id)
        {
            Id = id;
        }

        public bool IsSameId(object other)
        {
            var type = other?.GetType();
            var propertyInfo = type?.GetProperty("Id");
            var value = propertyInfo?.GetValue(other);
            return Id.Equals(value);
        }
    }

    public interface IIdentifiableDomainEvent
    {
        IIdentifiable EntityId { get; }
    }
}