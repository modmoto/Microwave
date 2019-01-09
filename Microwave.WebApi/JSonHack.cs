using System;
using System.Reflection;
using Microwave.Domain;
using Newtonsoft.Json.Linq;

namespace Microwave.WebApi
{
    public class JSonHack
    {
        public void SetEntityIdBackingField(JObject domainEventJobject, IDomainEvent domainEvent)
        {
            var field = domainEvent.GetType().GetField($"<{nameof(IDomainEvent.EntityId)}>k__BackingField",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var jToken = domainEventJobject.GetValue(nameof(IDomainEvent.EntityId), StringComparison.OrdinalIgnoreCase);
            field?.SetValue(domainEvent, jToken.ToString());
        }
    }
}