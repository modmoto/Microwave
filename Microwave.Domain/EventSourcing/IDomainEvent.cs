namespace Microwave.Domain.EventSourcing
{
    public interface IDomainEvent
    {
        string EntityId { get; }
    }
}