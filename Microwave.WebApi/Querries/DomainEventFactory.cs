using System;
using System.Collections.Generic;
using Microwave.Application;
using Microwave.Domain;
using Microwave.Queries;
using Microwave.WebApi.ApiFormatting.Identities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microwave.WebApi.Querries
{
    public interface IDomainEventFactory
    {
        IEnumerable<SubscribedDomainEventWrapper> Deserialize(string serializeObject);
    }

    public class DomainEventFactory : IDomainEventFactory
    {
        private readonly Dictionary<string, Type> _eventTypeRegistration;

        public DomainEventFactory(EventRegistration eventTypeRegistration)
        {
            _eventTypeRegistration = eventTypeRegistration;
        }

        public IEnumerable<SubscribedDomainEventWrapper> Deserialize(string serializeObject)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new IdentityConverter());
            var jArray = JsonConvert.DeserializeObject<JArray>(serializeObject);
            foreach (var jToken in jArray)
            {
                var jObject = (JObject) jToken;
                var value = jObject.GetValue(nameof(SubscribedDomainEventWrapper.DomainEventType), StringComparison.OrdinalIgnoreCase).Value<string>();
                if (!_eventTypeRegistration.ContainsKey(value)) continue;
                var type = _eventTypeRegistration[value];
                var version = jObject.GetValue(nameof(SubscribedDomainEventWrapper.Version), StringComparison.OrdinalIgnoreCase).Value<long>();
                var created = (DateTimeOffset) jObject.GetValue(nameof(SubscribedDomainEventWrapper.Created), StringComparison
                .OrdinalIgnoreCase);
                var domainEventJObject = jObject.GetValue(nameof(SubscribedDomainEventWrapper.DomainEvent), StringComparison
                .OrdinalIgnoreCase);
                var domainevent = (ISubscribedDomainEvent) domainEventJObject.ToObject(type, serializer);

                yield return new SubscribedDomainEventWrapper
                {
                    Created = created,
                    Version = version,
                    DomainEvent = domainevent
                };
            }
        }
    }
}