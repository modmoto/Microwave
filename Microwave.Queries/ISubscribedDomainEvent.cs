namespace Microwave.Queries
{
    public interface ISubscribedDomainEvent
    {
        string EntityId { get; }
    }
}