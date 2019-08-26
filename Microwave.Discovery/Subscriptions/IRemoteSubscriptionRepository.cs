using System;
using System.Threading.Tasks;

namespace Microwave.Discovery.Subscriptions
{
    public interface IRemoteSubscriptionRepository
    {
        Task SubscribeForEvent(Subscription subscription);
        Task PushChangesForType(Uri remoteService, string eventType, long newVersion);
        Task PushChangesForAll(Uri remoteService, DateTimeOffset newVersion);
    }
}