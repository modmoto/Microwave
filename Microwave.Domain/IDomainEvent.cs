namespace Microwave.Domain
{
    public interface IDomainEvent
    {
        Identity EntityId { get; }
    }
}