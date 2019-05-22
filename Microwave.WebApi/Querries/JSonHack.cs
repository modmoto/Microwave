using System;
using System.Reflection;
using Microwave.Application;
using Microwave.Domain;
using Newtonsoft.Json.Linq;

namespace Microwave.WebApi.Querries
{
    public class JSonHack
    {
        public void SetEntityIdBackingField(JObject domainEventJobject, ISubscribedDomainEvent domainEvent)
        {
            var field = domainEvent.GetType().GetField($"<{nameof(ISubscribedDomainEvent.EntityId)}>k__BackingField",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var jToken = domainEventJobject.GetValue(nameof(ISubscribedDomainEvent.EntityId), StringComparison.OrdinalIgnoreCase);
            var id = (string)jToken[nameof(Identity.Id)];
            field?.SetValue(domainEvent, Identity.Create(id));
        }
    }
}