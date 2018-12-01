using System;
using System.Reflection;
using Microwave.Application;
using Microwave.Domain;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microwave.ObjectPersistences
{
    public class ObjectConverter : IObjectConverter
    {
        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            ContractResolver = new PrivateSetterContractResolver()
        };

        public string Serialize<T>(T objectToSerialize)
        {
            return JsonConvert.SerializeObject(objectToSerialize, _settings);
        }

        public T Deserialize<T>(string payLoad)
        {
            var deserializeObject = JsonConvert.DeserializeObject<T>(payLoad, _settings);
            var domainEvent = deserializeObject as IDomainEvent;

            // this is for DomainEvents, where Newtonsoft can not find the EntityId in the constructor (because it is called differently)
            if (domainEvent != null && domainEvent.EntityId == new Guid())
            {
                var domainEventJobject = JToken.Parse(payLoad);
                var field = domainEvent.GetType().GetField($"<{nameof(IDomainEvent.EntityId)}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
                var jToken = domainEventJobject[nameof(IDomainEvent.EntityId)];
                var entityId = (Guid)jToken;
                field?.SetValue(domainEvent , entityId);
            }
            return deserializeObject;
        }
    }
}