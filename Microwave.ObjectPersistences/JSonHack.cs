using System;
using System.Reflection;
using Microwave.Domain;
using Newtonsoft.Json.Linq;

namespace Microwave.ObjectPersistences
{
    public class JSonHack
    {
        public void SetEntityIdBackingField(JObject domainEventJobject, IDomainEvent domainEvent)
        {
            var field = domainEvent.GetType().GetField($"<{nameof(IDomainEvent.EntityId)}>k__BackingField",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var jToken = domainEventJobject.GetValue(nameof(IDomainEvent.EntityId), StringComparison.OrdinalIgnoreCase);
            var entityId = (Guid) jToken;
            field?.SetValue(domainEvent, entityId);
        }
    }
}