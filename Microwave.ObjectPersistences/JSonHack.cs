using System;
using System.Reflection;
using Microwave.Domain;
using Newtonsoft.Json.Linq;

namespace Microwave.ObjectPersistences
{
    public class JSonHack
    {
        public void SetEntityIdBackingField(JToken domainEventJobject, IDomainEvent domainEvent)
        {
            var field = domainEvent.GetType().GetField($"<{nameof(IDomainEvent.EntityId)}>k__BackingField",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var jToken = domainEventJobject[nameof(IDomainEvent.EntityId)];
            var entityId = (Guid) jToken;
            field?.SetValue(domainEvent, entityId);
        }
    }
}