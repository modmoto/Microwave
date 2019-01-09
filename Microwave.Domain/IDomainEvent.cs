namespace Microwave.Domain
{
    public interface IDomainEvent
    {
        string EntityId { get; }
    }
}