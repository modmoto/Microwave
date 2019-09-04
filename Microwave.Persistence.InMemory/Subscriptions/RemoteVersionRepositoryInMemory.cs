using System;
using System.Threading.Tasks;
using Microwave.Queries.Ports;
using Microwave.Subscriptions;
using Microwave.Subscriptions.Ports;

namespace Microwave.Persistence.InMemory.Subscriptions
{
    public class RemoteVersionRepositoryInMemory : IRemoteVersionRepository
    {
        private readonly IVersionRepository _versionRepository;
        private readonly SharedMemoryClass _sharedMemoryClass;

        public RemoteVersionRepositoryInMemory(IVersionRepository versionRepository, SharedMemoryClass sharedMemoryClass)
        {
            _versionRepository = versionRepository;
            _sharedMemoryClass = sharedMemoryClass;
        }


        public async Task<SubscriptionState> GetSubscriptionState(Subscription subscription)
        {
            var currentVersion = await _versionRepository.GetVersionAsync(subscription.SubscribedEvent);
            return new SubscriptionState(currentVersion, default(DateTimeOffset));
        }

        public Task SaveRemoteVersionAsync(RemoteVersion version)
        {
            _sharedMemoryClass.Save(version);
            return Task.CompletedTask;

        }

        public Task SaveRemoteOverallVersionAsync(OverallVersion overallVersion)
        {
            throw new NotImplementedException();
        }
    }
}