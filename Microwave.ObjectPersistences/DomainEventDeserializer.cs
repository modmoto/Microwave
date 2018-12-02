using System;
using Microwave.Domain;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microwave.ObjectPersistences
{
    public class DomainEventDeserializer
    {
        private readonly JSonHack _jsonHack;

        public DomainEventDeserializer(JSonHack jsonHack)
        {
            _jsonHack = jsonHack;
        }

        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            ContractResolver = new PrivateSetterContractResolver()
        };

        public IDomainEvent Deserialize(string payLoad)
        {
            var domainEvent = JsonConvert.DeserializeObject<IDomainEvent>(payLoad, _settings);

            // this is for DomainEvents, where Newtonsoft can not find the EntityId in the constructor (because it is called differently)
            if (domainEvent.EntityId == new Guid())
            {
                var domainEventJobject = JObject.Parse(payLoad);
                _jsonHack.SetEntityIdBackingField(domainEventJobject, domainEvent);
            }

            return domainEvent;
        }
    }
}