using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Application.Framework;
using Domain.Framework;

namespace Adapters.Framework.EventStores
{
    public class HandlerVersionRepository : IHandlerVersionRepository
    {
        private string dbFolder = "JsonDB";

        private readonly Object _fileLock = new Object();


        public async Task<long> GetLastProcessedVersion(IEventHandler reactiveEventHandler, string name)
        {
            var path = $"JsonDB/{reactiveEventHandler.GetType().Name}-{name}";

            if (!File.Exists(path)) return 0;

            var content = await File.ReadAllLinesAsync(path);
            var wasLong = long.TryParse(content.FirstOrDefault(), out var version);
            if (!wasLong) return 0;
            return version;
        }

        public void IncrementProcessedVersion(IEventHandler reactiveEventHandler, DomainEvent domainEvent)
        {
            if (!Directory.Exists(dbFolder)) Directory.CreateDirectory(dbFolder);
            var path = $"{dbFolder}/{reactiveEventHandler.GetType().Name}-{domainEvent.GetType().Name}";

            if (!File.Exists(path)) File.Create(path);

            var content = File.ReadAllLines(path);
            var wasLong = long.TryParse(content.FirstOrDefault(), out var version);
            if (!wasLong) version = 0;

            File.WriteAllText(path, (version + 1).ToString());
        }
    }
}