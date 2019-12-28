using System;
using System.Threading.Tasks;
using Microwave.Domain.EventSourcing;
using Microwave.Queries;

namespace ModolithService
{
    public class Handler1 :
        IHandleAsync<Event2>,
        IHandleAsync<EventNotPublished>
    {
        public Task HandleAsync(Event2 domainEvent)
        {
            Console.WriteLine($"{DateTime.UtcNow.Second} Event2 was handled in Fast Handler");
            return Task.CompletedTask;
        }

        public Task HandleAsync(EventNotPublished domainEvent)
        {
            return Task.CompletedTask;
        }
    }

    public class Handler2 :
        IHandleAsync<Event2>
    {
        public Task HandleAsync(Event2 domainEvent)
        {
            Console.WriteLine($"{DateTime.UtcNow.Second} Event2 was handled in Sloooooow Handler 10 secs");
            return Task.CompletedTask;
        }
    }

    public class EventNotPublished : ISubscribedDomainEvent
    {
        public EventNotPublished(string entityId)
        {
            EntityId = entityId;
        }

        public string EntityId { get; }
    }

    public class ReadModel1 : ReadModel<Event2>, IHandle<Event2>
    {
        public void Handle(Event2 domainEvent)
        {
            Console.WriteLine($"{DateTime.UtcNow.Second} jeah 25 secs");
        }
    }

    public class Querry1 : Query, IHandle<Event2>
    {
        public void Handle(Event2 domainEvent)
        {
            Counter += 1;
            Console.WriteLine("jeah called 2");
        }

        public int Counter { get; set; }
    }

    public class ReadModelNotPublished : ReadModel<EventNotPublished>, IHandle<EventNotPublished>
    {
        public void Handle(EventNotPublished domainEvent)
        {
            Console.WriteLine("jeah called");
        }
    }

    public class Event1 : ISubscribedDomainEvent, IDomainEvent
    {
        public Event1(string entityId)
        {
            EntityId = entityId;
        }

        public string EntityId { get; }
    }


    public class Event2 : ISubscribedDomainEvent, IDomainEvent
    {
        public Event2(string entityId, string additionalInfo)
        {
            EntityId = entityId;
            AdditionalInfo = additionalInfo;
        }


        public string Name { get; }
        public string SurName { get; }

        public string EntityId { get; }
        public string AdditionalInfo { get; }
    }
}