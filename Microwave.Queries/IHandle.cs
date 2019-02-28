using Microwave.Domain;

namespace Microwave.Queries
{
    public interface IHandle<in T> where T : IDomainEvent
    {
        void Handle(T domainEvent);
    }

    public interface IHandleVersioned<in T> where T : IDomainEvent
    {
        void Handle(T domainEvent, long version);
    }
}