using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microwave.Domain.Identities;
using Microwave.EventStores.Ports;

namespace Microwave.Persistence.CosmosDb
{
   public class CosmosDbSnapshotRepository : ISnapShotRepository
    {
        private readonly ICosmosDbClient _cosmosDbClient;

        public CosmosDbSnapshotRepository(ICosmosDbClient cosmosDbClient)
        {
            _cosmosDbClient = cosmosDbClient;
        }

        public async Task<SnapShotResult<T>> LoadSnapShot<T>(Identity entityId) where T : new()
        {
            var result = _cosmosDbClient.LoadSnapshotAsync<T>(entityId);
            return result.Result;
        }

        public Task SaveSnapShot<T>(SnapShotWrapper<T> snapShot)
        {
            throw new NotImplementedException();
        }
    }
}
