using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Domain.Framework;
using Newtonsoft.Json;

namespace Adapters.Framework.EventStores
{
    public class DomainEventPersister : IDomainEventPersister
    {
        private readonly string _filePath;

        public DomainEventPersister(string filePath)
        {
            _filePath = filePath;
        }

        public async Task Store(IEnumerable<DomainEvent> domainEvents)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            var serializeObject = JsonConvert.SerializeObject(domainEvents, settings);
            await File.WriteAllTextAsync(_filePath, serializeObject);
        }

        public IEnumerable<DomainEvent> Load()
        {
            var readAllText = File.ReadAllText(_filePath);
            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            return JsonConvert.DeserializeObject<IEnumerable<DomainEvent>>(readAllText, settings);
        }
    }
}