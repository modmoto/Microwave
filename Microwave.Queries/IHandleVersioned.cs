namespace Microwave.Queries
{
    public interface IHandleVersioned<in T> : ISubscribedDomainEvent
    {
        void Handle(T domainEvent, long version);
    }
}