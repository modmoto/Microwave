using System.Collections.Generic;
using Application.Framework;
using Domain.Framework;
using Newtonsoft.Json;

namespace Adapters.Json.ObjectPersistences
{
    public class DomainEventConverter : IDomainEventConverter
    {
        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            ContractResolver = new PrivateSetterContractResolver(),
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
        };

        public string Serialize<T>(T eve) where T : DomainEvent
        {
            return JsonConvert.SerializeObject(eve, _settings);
        }

        public T Deserialize<T>(string payLoad)
        {
            var deserializeObject = JsonConvert.DeserializeObject<T>(payLoad, _settings);
            return deserializeObject;
        }

        public IEnumerable<T> DeserializeList<T>(string payLoad)
        {
            var deserializeObject = JsonConvert.DeserializeObject<IEnumerable<T>>(payLoad, _settings);
            return deserializeObject;
        }
    }
}