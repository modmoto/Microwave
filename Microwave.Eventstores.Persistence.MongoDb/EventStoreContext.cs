using System;
using Microwave.Domain.EventSourcing;

namespace Microwave.Eventstores.Persistence.MongoDb
{
    public class SnapShotDbo<T>
    {
        public string Id { get; set; }
        public T Payload { get; set; }
        public long Version { get; set; }
    }

    public class DomainEventDbo
    {
        public DomainEventKey Key { get; set; }

        public string Id {
            get => $"{Key.EntityId}_StreamVersion:_{Key.Version}";
            set => Key = new DomainEventKey
            {
                EntityId = value.Split(new[] { "_StreamVersion:_" }, StringSplitOptions.None)[0],
                Version = long.Parse(value.Split(new[] { "_StreamVersion:_" }, StringSplitOptions.None)[1])
            };
        }
        public IDomainEvent Payload { get; set; }
        public DateTimeOffset Created { get; set; }
        public string EventType { get; set; }
    }

    public class DomainEventKey
    {
        public string EntityId { get; set; }
        public long Version { get; set; }
    }
}