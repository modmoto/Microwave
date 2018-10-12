using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Framework;

namespace Application.Framework
{
    public abstract class ReactiveEventHandler<T> : IReactiveEventHandler
    {
        private readonly IHandlerVersionRepository _versionRepository;
        private readonly Object _fileLock = new Object();

        public ReactiveEventHandler(SubscribedEventTypes<T> subscribedEventTypes, IHandlerVersionRepository versionRepository)
        {
            _versionRepository = versionRepository;
            SubscribedDomainEventTypes = subscribedEventTypes;
        }

        public async Task Handle(DomainEvent domainEvent, StreamVersion streamVersion)
        {
            lock (_fileLock)
            {
                var type = domainEvent.GetType();
                var currentEntityType = GetType();
                var methodInfos = currentEntityType.GetMethods().Where(method => method.Name == nameof(Handle));
                var methodToExecute = methodInfos.FirstOrDefault(method => method.GetParameters().FirstOrDefault()?.ParameterType == type);
                if (methodToExecute == null || methodToExecute.GetParameters().Length != 1) return;
                methodToExecute.Invoke(this, new object[] {domainEvent});
                _versionRepository.IncrementProcessedVersion(this, domainEvent, streamVersion);
            }
        }

        public IEnumerable<string> SubscribedDomainEventTypes { get; }
    }
}