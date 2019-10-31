using System.Threading.Tasks;
using Microwave.Domain.EventSourcing;
using Microwave.Queries;

namespace WriteService2
{
    public class Handler1 : IHandleAsync<Event2>
    {
        public Task HandleAsync(Event2 domainEvent)
        {
            return Task.CompletedTask;
        }
    }
    public class Entity2 : Entity, IApply<Event3>, IApply<Event4>
    {
        public void Apply(Event3 domainEvent)
        {
        }

        public void Apply(Event4 domainEvent)
        {
        }
    }
    public class Event3 : IDomainEvent
    {
        public Event3(string entityId)
        {
            EntityId = entityId;
        }

        public string EntityId { get; }
    }


    public class Event4 : IDomainEvent
    {
        public Event4(string entityId)
        {
            EntityId = entityId;
        }

        public int Age { get; }
        public string Name { get; }

        public string EntityId { get; }
    }
    
    public class Event2 : ISubscribedDomainEvent
    {
        public Event2(string entityId)
        {
            EntityId = entityId;
        }

        public string EntityId { get; }
    }

    //Event that is not used by Entity for Test
    public class Event1 : IDomainEvent
    {
        public Event1(string entityId)
        {
            EntityId = entityId;
        }

        public string EntityId { get; }
    }
}