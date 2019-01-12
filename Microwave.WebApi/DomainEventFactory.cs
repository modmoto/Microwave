using System;
using System.Collections.Generic;
using Microwave.Application;
using Microwave.Domain;
using Microwave.WebApi.ApiFormatting.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microwave.WebApi
{
    public interface IDomainEventFactory
    {
        IEnumerable<DomainEventWrapper> Deserialize(string serializeObject);
    }

    public class DomainEventFactory : IDomainEventFactory
    {
        private readonly Dictionary<string, Type> _eventTypeRegistration;

        public DomainEventFactory(EventRegistration eventTypeRegistration)
        {
            _eventTypeRegistration = eventTypeRegistration;
        }

        public IEnumerable<DomainEventWrapper> Deserialize(string serializeObject)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new IdentityConverter());
            var jArray = JsonConvert.DeserializeObject<JArray>(serializeObject);
            foreach (var jToken in jArray)
            {
                var jObject = (JObject) jToken;
                var value = jObject.GetValue(nameof(DomainEventWrapper.DomainEventType), StringComparison.OrdinalIgnoreCase).Value<string>();
                if (!_eventTypeRegistration.ContainsKey(value)) continue;
                var type = _eventTypeRegistration[value];
                var version = jObject.GetValue(nameof(DomainEventWrapper.Version), StringComparison.OrdinalIgnoreCase).Value<long>();
                var created = (DateTimeOffset) jObject.GetValue(nameof(DomainEventWrapper.Created), StringComparison
                .OrdinalIgnoreCase);
                var domainEventJObject = jObject.GetValue(nameof(DomainEventWrapper.DomainEvent), StringComparison.OrdinalIgnoreCase);
                var domainevent = (IDomainEvent) domainEventJObject.ToObject(type, serializer);

                yield return new DomainEventWrapper
                {
                    Created = created,
                    Version = version,
                    DomainEvent = domainevent
                };
            }
        }
    }
}