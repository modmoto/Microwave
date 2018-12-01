using System;
using System.Collections.Generic;
using System.Linq;
using Microwave.Application;
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

        public IEnumerable<DomainEventWrapper> Deserialize(string payLoad)
        {
            var list = JsonConvert.DeserializeObject<IEnumerable<DomainEventWrapper>>(payLoad, _settings);
            var domainEventJobjectStuff = JToken.Parse(payLoad);
            var jobjectList = domainEventJobjectStuff["$values"];

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