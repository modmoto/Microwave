using System;
using System.Threading.Tasks;

namespace Microwave.Subscriptions.Ports
{
    public interface IRemoteSubscriptionRepository
    {
        Task SubscribeForEvent(Subscription subscription);
        Task PushChangesForType(Uri remoteService, string eventType, DateTimeOffset newVersion);
        Task PushChangesForAll(Uri remoteService, DateTimeOffset newVersion);
    }
}