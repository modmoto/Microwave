using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microwave.Queries.Ports;

[assembly: InternalsVisibleTo("Microwave")]
[assembly: InternalsVisibleTo("Microwave.WebApi.UnitTests")]
[assembly: InternalsVisibleTo("Microwave.UnitTests")]
[assembly: InternalsVisibleTo("Microwave.WebApi")]
namespace Microwave.Queries.Handler
{
    public class AsyncEventHandler<T> : IAsyncEventHandler where T : ISubscribedDomainEvent
    {
        private readonly IEventFeed<AsyncEventHandler<T>>  _eventFeed;
        private readonly IHandleAsync<T> _handler;
        private readonly IVersionRepository _versionRepository;
        public Type HandlerClassType => _handler.GetType();

        public AsyncEventHandler(
            IVersionRepository versionRepository,
            IEventFeed<AsyncEventHandler<T>> eventFeed,
            IHandleAsync<T> handler)
        {
            _versionRepository = versionRepository;
            _eventFeed = eventFeed;
            _handler = handler;
        }

        public async Task Update()
        {
            var handleType = _handler.GetType();
            var domainEventType = $"{handleType.Name}-{typeof(T).Name}";
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
                await _versionRepository.SaveVersion(new LastProcessedVersion(domainEventType, latestEvent.Created));
            }
        }
    }
}