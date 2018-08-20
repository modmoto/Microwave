using System.IO;
using System.Threading.Tasks;
using Application.Framework;
using Newtonsoft.Json;

namespace Adapters.Json.ObjectPersistences
{
    public abstract class ObjectPersister<T> : IObjectPersister<T>
    {
        private readonly string _filePath;

        public ObjectPersister(string filePath)
        {
            _filePath = filePath;
        }

        public async Task<T> GetAsync()
        {
            var readAllText = await File.ReadAllTextAsync(_filePath);
            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            return JsonConvert.DeserializeObject<T>(readAllText, settings);
        }

        public async Task Save(T querry)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            var serializeObject = JsonConvert.SerializeObject(querry, settings);
            await File.WriteAllTextAsync(_filePath, serializeObject);
        }
    }
}