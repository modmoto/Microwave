using System;
using System.Threading.Tasks;

namespace Microwave.Subscriptions.Ports
{
    public interface IRemoteVersionRepository
    {
        Task<SubscriptionState> GetSubscriptionState(Subscription subscription);
        Task SaveRemoteVersionAsync(RemoteVersion version);
        Task SaveRemoteOverallVersionAsync(OverallVersion overallVersion);
        Task<DateTimeOffset> GetLastVersionOfEventType(Subscription subscription);
        Task<DateTimeOffset> GetCurrentVersionOfEventType(Subscription subscription);
        Task SaveCurrentVersionAsLastVersion(string eventType, DateTimeOffset newVersion);
    }
}