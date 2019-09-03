using System;
using System.Linq;
using System.Threading.Tasks;
using Microwave.Queries.Ports;

namespace Microwave.Queries.Handler
{
    public class AsyncEventHandler<T> : IAsyncEventHandler where T : ISubscribedDomainEvent
    {
        private readonly IEventFeed<AsyncEventHandler<T>>  _eventFeed;
        private readonly IHandleAsync<T> _handler;
        private readonly IVersionRepository _versionRepository;
        private readonly IRemoteVersionReadRepository _remoteVersionReadRepository;
        public Type HandlerClassType => _handler.GetType();

        public AsyncEventHandler(
            IVersionRepository versionRepository,
            IRemoteVersionReadRepository remoteVersionReadRepository,
            IEventFeed<AsyncEventHandler<T>> eventFeed,
            IHandleAsync<T> handler)
        {
            _versionRepository = versionRepository;
            _remoteVersionReadRepository = remoteVersionReadRepository;
            _eventFeed = eventFeed;
            _handler = handler;
        }

        public async Task Update()
        {
            var handleType = _handler.GetType();
            var domainEventType = $"{handleType.Name}-{typeof(T).Name}";
            var lastVersion = await _versionRepository.GetVersionAsync(domainEventType);
            var lastVersionRemote = await _remoteVersionReadRepository.GetVersionAsync(domainEventType);
            if (lastVersion > lastVersionRemote) return;
            var latestEvents = await _eventFeed.GetEventsAsync(lastVersion);
            foreach (var latestEvent in latestEvents)
            {
                var methodInfos = handleType.GetMethods();
                var methodInfo = methodInfos.FirstOrDefault(m => m.GetParameters().Length == 1
                                                                 && m.Name == "HandleAsync"
                                                                 && m.GetParameters().First().ParameterType == latestEvent.DomainEvent.GetType());
                if (methodInfo == null) continue;
                await (Task) methodInfo.Invoke(_handler, new object[] { latestEvent.DomainEvent });
                await _versionRepository.SaveVersionAsync(new LastProcessedVersion(domainEventType, latestEvent.Created));
            }
        }
    }
}