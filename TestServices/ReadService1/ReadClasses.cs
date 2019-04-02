using System;
using System.Threading.Tasks;
using Microwave.Domain;
using Microwave.Queries;

namespace ReadService1
{
    public class Handler1 : IHandleAsync<Event2>, IHandleAsync<Event4>
    {
        public Task HandleAsync(Event2 domainEvent)
        {
            return null;
        }

        public Task HandleAsync(Event4 domainEvent)
        {
            return null;
        }
    }

    public class ReadModel1 : ReadModel, IHandle<Event3>, IHandle<Event4>
    {
        public override Type GetsCreatedOn => typeof(Event3);

        public void Handle(Event3 domainEvent)
        {
        }

        public void Handle(Event4 domainEvent)
        {
        }
    }

    public class Event1 : IDomainEvent
    {
        public Event1(Identity entityId)
        {
            EntityId = entityId;
        }

        public Identity EntityId { get; }
    }


    public class Event2 : IDomainEvent
    {
        public Event2(Identity entityId)
        {
            EntityId = entityId;
        }

        public Identity EntityId { get; }
    }

    public class Event3 : IDomainEvent
    {
        public Event3(Identity entityId)
        {
            EntityId = entityId;
        }

        public Identity EntityId { get; }
    }


    public class Event4 : IDomainEvent
    {
        public Event4(Identity entityId)
        {
            EntityId = entityId;
        }

        public Identity EntityId { get; }
    }
}