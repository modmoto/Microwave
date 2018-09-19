using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Framework;

namespace Application.Framework
{
    public class ReactiveEventHandler<T> : IEventHandler
    {
        public ReactiveEventHandler(SubscribedEventTypes<T> subscribedEventTypes)
        {
            SubscribedDomainEventTypes = subscribedEventTypes;
        }

        public void Handle(DomainEvent domainEvent)
        {
            var type = domainEvent.GetType();
            var currentEntityType = GetType();
            var methodInfos = currentEntityType.GetMethods().Where(method => method.Name == nameof(Handle));
            var methodToExecute = methodInfos.FirstOrDefault(method => method.GetParameters().FirstOrDefault()?.ParameterType == type);
            if (methodToExecute == null || methodToExecute.GetParameters().Length != 1) return;
            methodToExecute.Invoke(this, new object[] {domainEvent});
        }

        public IEnumerable<Type> SubscribedDomainEventTypes { get; }
    }
}