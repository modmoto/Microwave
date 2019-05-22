using System;
using System.Threading.Tasks;
using Microwave.Application;
using Microwave.Domain;
using Microwave.Queries;

namespace ReadService1
{
    public class Handler1 : IHandleAsync<Event2>, IHandleAsync<Event4>, IHandleAsync<EventNotPublished>
    {
        public Task HandleAsync(Event2 domainEvent)
        {
            return Task.CompletedTask;
        }

        public Task HandleAsync(Event4 domainEvent)
        {
            return Task.CompletedTask;
        }

        public Task HandleAsync(EventNotPublished domainEvent)
        {
            return Task.CompletedTask;
        }
    }

    public class EventNotPublished : ISubscribedDomainEvent
    {
        public EventNotPublished(Identity entityId)
        {
            EntityId = entityId;
        }

        public Identity EntityId { get; }
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

    public class ReadModelNotPublished : ReadModel, IHandle<EventNotPublished>
    {
        public override Type GetsCreatedOn => typeof(EventNotPublished);

        public void Handle(EventNotPublished domainEvent)
        {
        }
    }

    public class Event1 : ISubscribedDomainEvent
    {
        public Event1(Identity entityId)
        {
            EntityId = entityId;
        }

        public Identity EntityId { get; }
    }


    public class Event2 : ISubscribedDomainEvent
    {
        public Event2(Identity entityId)
        {
            EntityId = entityId;
        }


        public string Name { get; }
        public string SurName { get; }

        public Identity EntityId { get; }
    }

    public class Event3 : ISubscribedDomainEvent
    {
        public Event3(Identity entityId)
        {
            EntityId = entityId;
        }

        public string RmName { get; }
        public string RmSurName { get; }
        public Identity EntityId { get; }
    }


    public class Event4 : ISubscribedDomainEvent
    {
        public Event4(Identity entityId)
        {
            EntityId = entityId;
        }

        public int Age { get; }
        public string Name { get; }
        public string WeirdName { get; }

        public Identity EntityId { get; }
    }
}