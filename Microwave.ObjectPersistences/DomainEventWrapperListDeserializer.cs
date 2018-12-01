using System;
using System.Collections.Generic;
using System.Linq;
using Microwave.Application;
using Microwave.Domain;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microwave.ObjectPersistences
{
    public class DomainEventWrapperListDeserializer
    {
        private readonly JSonHack _jsonHack;

        public DomainEventWrapperListDeserializer(JSonHack jsonHack)
        {
            _jsonHack = jsonHack;
        }

        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            ContractResolver = new PrivateSetterContractResolver()
        };

        public IEnumerable<DomainEventWrapper<T>> Deserialize<T>(string payLoad) where T : IDomainEvent
        {
            var list = JsonConvert.DeserializeObject<IEnumerable<DomainEventWrapper<T>>>(payLoad, _settings);
            var domainEventJobjectStuff = JToken.Parse(payLoad);
            var jobjectList = domainEventJobjectStuff.ToList();

            var domainEventWrappers = list.ToList();
            for (int i = 0; i < domainEventWrappers.Count; i++)
            {
                var domainEventWrapper = domainEventWrappers[i];
                if (domainEventWrapper.DomainEvent.EntityId == new Guid())
                {
                    var jTokenDomainEvent = jobjectList[i][nameof(domainEventWrapper.DomainEvent)];
                    _jsonHack.SetEntityIdBackingField(jTokenDomainEvent, domainEventWrapper.DomainEvent);
                }
            }

            return domainEventWrappers;
        }
    }
}