namespace Microwave.Domain.EventSourcing
{
    public interface IDomainEvent : IIdentifiable
    {
    }

    public interface IIdentifiable
    {
        string EntityId { get; }
    }
}