using System.Collections.Generic;
using Application.Framework;
using Newtonsoft.Json;

namespace Adapters.Json.ObjectPersistences
{
    public class ObjectConverter : IObjectConverter
    {
        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            ContractResolver = new PrivateSetterContractResolver()
        };

        public string Serialize<T>(T eve)
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