using System.Threading.Tasks;

namespace Microwave.Subscriptions.Ports
{
    public interface IRemoteVersionRepository
    {
        Task<SubscriptionState> GetSubscriptionState(Subscription subscription);
        Task SaveRemoteVersionAsync(RemoteVersion version);
        Task SaveRemoteOverallVersionAsync(OverallVersion overallVersion);
    }
}