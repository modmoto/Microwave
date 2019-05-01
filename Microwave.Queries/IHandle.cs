namespace Microwave.Queries
{
    public interface IHandle<in T>
    {
        void Handle(T domainEvent);
    }

    public interface IHandleVersioned<in T>
    {
        void Handle(T domainEvent, long version);
    }
}