using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Adapters.Json.ObjectPersistences
{
    public abstract class ObjectPersister : IObjectPersister
    {
        private readonly string _filePath;

        public ObjectPersister(string filePath)
        {
            _filePath = filePath;
        }

        public async Task<T> GetAsync<T>()
        {
            var readAllText = await File.ReadAllTextAsync(_filePath);
            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            return JsonConvert.DeserializeObject<T>(readAllText, settings);
        }

        public async Task Save<T>(T querry)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            var serializeObject = JsonConvert.SerializeObject(querry, settings);
            await File.WriteAllTextAsync(_filePath, serializeObject);
        }
    }
}