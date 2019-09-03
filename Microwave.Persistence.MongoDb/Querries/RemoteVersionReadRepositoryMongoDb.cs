using System;
using System.Threading.Tasks;
using Microwave.Queries.Ports;
using MongoDB.Driver;

namespace Microwave.Persistence.MongoDb.Querries
{
    public class RemoteVersionReadRepositoryMongoDb : IRemoteVersionReadRepository
    {
        private readonly IMongoDatabase _dataBase;
        private readonly string _lastProcessedVersions = "LastProcessedRemoteVersions";

        public RemoteVersionReadRepositoryMongoDb(MicrowaveMongoDb dataBase)
        {
            _dataBase = dataBase.Database;
        }

        public async Task<DateTimeOffset> GetVersionAsync(string domainEventType)
        {
            var mongoCollection = _dataBase.GetCollection<LastProcessedVersionDbo>(_lastProcessedVersions);
            var lastProcessedVersion =
                (await mongoCollection.FindAsync(version => version.EventType == domainEventType)).FirstOrDefault();
            return lastProcessedVersion?.LastVersion ?? DateTimeOffset.MinValue;
        }
    }
}