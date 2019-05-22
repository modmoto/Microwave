using Microwave.Application;

namespace Microwave.Queries
{
    public interface IHandle<in T> where T : ISubscribedDomainEvent
    {
        void Handle(T domainEvent);
    }

    public interface IHandleVersioned<in T> : ISubscribedDomainEvent
    {
        void Handle(T domainEvent, long version);
    }
}