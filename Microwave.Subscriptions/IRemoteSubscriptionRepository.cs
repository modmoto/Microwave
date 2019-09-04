using System;
using System.Threading.Tasks;

namespace Microwave.Subscriptions
{
    public interface IRemoteSubscriptionRepository
    {
        Task SubscribeForEvent(Subscription subscription);
        Task PushChangesForType(Uri remoteService, string eventType, DateTimeOffset newVersion);
        Task PushChangesForAll(Uri remoteService, DateTimeOffset newVersion);
    }
}