using System;
using Microwave.Domain.EventSourcing;

namespace Microwave.Persistence.MongoDb.Eventstores
{
    internal class SnapShotDbo<T>
    {
        public string Id { get; set; }
        public T Payload { get; set; }
        public long Version { get; set; }
    }

    internal class DomainEventDbo
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

    internal class DomainEventKey
    {
        public string EntityId { get; set; }
        public long Version { get; set; }
    }
}