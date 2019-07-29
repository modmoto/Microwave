namespace Microwave.Queries
{
    public interface IHandle<in T> where T : ISubscribedDomainEvent
    {
        void Handle(T domainEvent);
    }
}