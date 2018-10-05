using System.Text;
using Application.Framework;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace Adapters.Json.ObjectPersistences
{
    public class JSonConverter<T> : IJSonConverter<T>
    {
        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            ContractResolver = new PrivateSetterContractResolver(),
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
        };

        public string Serialize(T eve)
        {
            return JsonConvert.SerializeObject(eve, _settings);
        }

        public T Deserialize(ResolvedEvent domainEvent)
        {
            var eventData = Encoding.UTF8.GetString(domainEvent.Event.Data);
            var deserializeObject = JsonConvert.DeserializeObject<T>(eventData, _settings);
            return deserializeObject;
        }
    }
}