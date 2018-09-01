using System.IO;
using System.Threading.Tasks;
using Application.Framework;
using Newtonsoft.Json;

namespace Adapters.Json.ObjectPersistences
{
    public class ObjectPersister<T> : IObjectPersister<T>
    {
        private readonly string _filePath;

        public ObjectPersister()
        {
            if (!Directory.Exists("JsonDB")) Directory.CreateDirectory("JsonDB");
            _filePath = $"JsonDB/DB_{typeof(T).Name}_{typeof(T).FullName}.json";
        }

        public async Task<T> GetAsync()
        {
            if (!File.Exists(_filePath)) return default(T);
            var readAllText = await File.ReadAllTextAsync(_filePath);
            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All, ContractResolver = new PrivateContractResolver() };
            return JsonConvert.DeserializeObject<T>(readAllText, settings);
        }

        public async Task Save(T querry)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All, ContractResolver = new PrivateContractResolver()};
            var serializeObject = JsonConvert.SerializeObject(querry, settings);
            await File.WriteAllTextAsync(_filePath, serializeObject);
        }
    }
}