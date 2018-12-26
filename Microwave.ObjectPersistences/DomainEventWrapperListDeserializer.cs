using System;
using System.Collections.Generic;
using System.Linq;
using Microwave.Application;
using Newtonsoft.Json.Linq;

namespace Microwave.ObjectPersistences
{
    public class DomainEventWrapperListDeserializer
    {
        private readonly JSonHack _jsonHack;
        private readonly IDomainEventFactory _domainEventFactory;

        public DomainEventWrapperListDeserializer(JSonHack jsonHack, IDomainEventFactory domainEventFactory)
        {
            _jsonHack = jsonHack;
            _domainEventFactory = domainEventFactory;
        }

        public IEnumerable<DomainEventWrapper> Deserialize(string payLoad)
        {
            var domainEventWrappers = _domainEventFactory.Deserialize(payLoad);
            var domainEventJobjectStuff = JToken.Parse(payLoad);
            var jobjectList = domainEventJobjectStuff.ToObject<List<JObject>>();

            var eventWrappers = domainEventWrappers.ToList();
            for (int i = 0; i < eventWrappers.Count; i++)
            {
                var domainEventWrapper = eventWrappers[i];
                if (domainEventWrapper.DomainEvent.EntityId?.Id == domainEventWrapper.DomainEvent.EntityId?.DefaultValue)
                {
                    var jTokenDomainEvent = jobjectList[i].GetValue(nameof(domainEventWrapper.DomainEvent),
                        StringComparison.OrdinalIgnoreCase).ToObject<JObject>();
                    _jsonHack.SetEntityIdBackingField(jTokenDomainEvent, domainEventWrapper.DomainEvent);
                }
            }

            return eventWrappers;
        }
    }
}