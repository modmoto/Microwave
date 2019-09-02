using System.Threading.Tasks;
using Microwave.Discovery.EventLocations;

namespace Microwave.Discovery.Subscriptions
{
    public class SubscriptionHandler : ISubscriptionHandler
    {
        private readonly EventsSubscribedByService _eventsSubscribedByService;
        private readonly IStatusRepository _statusRepository;
        private readonly IRemoteSubscriptionRepository _remoteSubscriptionRepository;
        private readonly ISubscriptionRepository _subscriptionRepository;

        public SubscriptionHandler(
            EventsSubscribedByService eventsSubscribedByService,
            IStatusRepository statusRepository,
            IRemoteSubscriptionRepository remoteSubscriptionRepository,
            ISubscriptionRepository subscriptionRepository)
        {
            _eventsSubscribedByService = eventsSubscribedByService;
            _statusRepository = statusRepository;
            _remoteSubscriptionRepository = remoteSubscriptionRepository;
            _subscriptionRepository = subscriptionRepository;
        }

        public async Task SubscribeOnDiscoveredServices()
        {
            var eventLocation = await _statusRepository.GetEventLocation();
            foreach (var subscribedEvent in _eventsSubscribedByService.Events)
            {
                var microwaveServiceNode = eventLocation.GetServiceForEvent(subscribedEvent.Name);
                if (microwaveServiceNode == null) continue;
                await _remoteSubscriptionRepository.SubscribeForEvent(
                    new Subscription(subscribedEvent.Name, microwaveServiceNode.ServiceEndPoint.ServiceBaseAddress));
            }
        }

        public async Task StoreSubscription(Subscription subscription)
        {
            await _subscriptionRepository.StoreSubscriptionAsync(subscription);
        }

        public async Task PushNewChanges()
        {
            var subscriptions = await _subscriptionRepository.LoadSubscriptionsAsync();
            foreach (var subscription in subscriptions)
            {
                var newVersion = await _subscriptionRepository.GetCurrentVersion(subscription);
                await _remoteSubscriptionRepository.PushChangesForType(
                    subscription.SubscriberUrl,
                    subscription.SubscribedEvent,
                    newVersion);
            }
        }
    }
}