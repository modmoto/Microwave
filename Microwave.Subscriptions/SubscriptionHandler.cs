using System.Collections.Generic;
using System.Threading.Tasks;
using Microwave.Subscriptions.Ports;
using Microwave.Subscriptions.ReadModels;

namespace Microwave.Subscriptions
{
    public class SubscriptionHandler : ISubscriptionHandler
    {
        private readonly EventsSubscribedByServiceReadModel _eventsSubscribedByService;
        private readonly IEventLocationReadModelRepository _statusRepository;
        private readonly IRemoteSubscriptionRepository _remoteSubscriptionRepository;
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IRemoteVersionRepository _remoteVersionRepository;

        public SubscriptionHandler(
            EventsSubscribedByServiceReadModel eventsSubscribedByService,
            IEventLocationReadModelRepository statusRepository,
            IRemoteSubscriptionRepository remoteSubscriptionRepository,
            ISubscriptionRepository subscriptionRepository,
            IRemoteVersionRepository remoteVersionRepository)
        {
            _eventsSubscribedByService = eventsSubscribedByService;
            _statusRepository = statusRepository;
            _remoteSubscriptionRepository = remoteSubscriptionRepository;
            _subscriptionRepository = subscriptionRepository;
            _remoteVersionRepository = remoteVersionRepository;
        }

        public async Task SubscribeOnDiscoveredServices()
        {
            var eventLocation = await _statusRepository.GetEventLocation();
            foreach (var subscribedEvent in _eventsSubscribedByService.Events)
            {
                var microwaveServiceNode = eventLocation.GetServiceForEvent(subscribedEvent.Name);
                if (microwaveServiceNode == null) continue;
                await _remoteSubscriptionRepository.SubscribeForEvent(
                    new Subscription(subscribedEvent.Name, microwaveServiceNode.ServiceBaseAddress));
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
                var newVersion = await _remoteVersionRepository.GetSubscriptionState(subscription);
                if (!newVersion.NeedsPush) continue;
                var result = await _remoteSubscriptionRepository.PushChangesForType(
                    subscription.SubscriberUrl,
                    subscription.SubscribedEvent,
                    newVersion.NewVersion);
                if (result)
                {
                    // new version speichern
                }
            }
        }

        public Task<IEnumerable<Subscription>> GetSubscriptions()
        {
            return _subscriptionRepository.LoadSubscriptionsAsync();
        }

        public Task StoreNewRemoteVersion(StoreNewVersionCommand command)
        {
            return _remoteVersionRepository.SaveRemoteVersionAsync(new RemoteVersion(command.EventType, command.NewVersion));
        }

        public Task StoreNewRemoteOverallVersion(StoreNewOverallVersionCommand command)
        {
            return _remoteVersionRepository.SaveRemoteOverallVersionAsync(
                new OverallVersion(command.ServiceUri, command.NewVersion));
        }
    }
}