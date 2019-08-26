using System.Collections.Generic;
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
            await _subscriptionRepository.StoreSubscription(subscription);
        }

        public async Task PushNewChanges()
        {
            var subscriptions = await _subscriptionRepository.LoadSubscriptions();
            foreach (var subscription in subscriptions)
            {
                long newVersion = 0; // get from versionRepo
                await _remoteSubscriptionRepository.PushChangesForType(
                    subscription.SubscriberUrl,
                    subscription.SubscribedEvent,
                    newVersion);
            }
        }
    }

    public interface ISubscriptionRepository
    {
        Task StoreSubscription(Subscription subscription);
        Task<IEnumerable<Subscription>> LoadSubscriptions();
    }
}