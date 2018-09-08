using System.Text;
using Application.Framework;
using Domain.Framework;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace Adapters.Json.ObjectPersistences
{
    public class DomainEventConverter : IDomainEventConverter
    {
        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All, ContractResolver = new PrivateSetterContractResolver() };

        public string Serialize(DomainEvent domainEvent)
        {
            return JsonConvert.SerializeObject(domainEvent, _settings);
        }

        public DomainEvent Deserialize(ResolvedEvent domaiEvent)
        {
            var eventData = Encoding.UTF8.GetString(domaiEvent.Event.Data);
            var deserializeObject = JsonConvert.DeserializeObject<DomainEvent>(eventData, _settings);
            return deserializeObject;
        }
    }
}