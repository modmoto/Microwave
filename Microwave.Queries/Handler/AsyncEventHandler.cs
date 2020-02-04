using System;
using System.Linq;
using System.Threading.Tasks;
using Microwave.Queries.Ports;

namespace Microwave.Queries.Handler
{
    public class AsyncEventHandler<TSubscribedDomainEvent, TConcreteHandlerType>
        : IEventHandler where TSubscribedDomainEvent : ISubscribedDomainEvent
    {
        private readonly IEventFeed<AsyncEventHandler<TSubscribedDomainEvent, TConcreteHandlerType>>  _eventFeed;
        private readonly IHandleAsync<TSubscribedDomainEvent> _handler;
        private readonly IVersionRepository _versionRepository;
        public Type HandlerClassType => _handler.GetType();

        public AsyncEventHandler(
            IVersionRepository versionRepository,
            IEventFeed<AsyncEventHandler<TSubscribedDomainEvent, TConcreteHandlerType>> eventFeed,
            IHandleAsync<TSubscribedDomainEvent> handler)
        {
            _versionRepository = versionRepository;
            _eventFeed = eventFeed;
            _handler = handler;
        }

        public async Task Update()
        {
            var handleType = _handler.GetType();
            var domainEventType = $"{handleType.Name}-{typeof(TSubscribedDomainEvent).Name}";
            var lastVersion = await _versionRepository.GetVersionAsync(domainEventType);
            var latestEvents = await _eventFeed.GetEventsAsync(lastVersion);
            foreach (var latestEvent in latestEvents)
            {
                var methodInfos = handleType.GetMethods();
                var methodInfo = methodInfos.FirstOrDefault(m => m.GetParameters().Length == 1
                                                                 && m.Name == "HandleAsync"
                                                                 && m.GetParameters().First().ParameterType == latestEvent.DomainEvent.GetType());
                if (methodInfo == null) continue;
                await (Task) methodInfo.Invoke(_handler, new object[] { latestEvent.DomainEvent });
                await _versionRepository.SaveVersion(new LastProcessedVersion(domainEventType, latestEvent.OverallVersion));
            }
        }
    }
}