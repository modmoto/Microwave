using System;
using Domain.Framework;

namespace Application.Framework
{
    public interface IEventStoreSubscribtion
    {
        void SubscribeFrom(string domainEventType, long version, Action<DomainEvent> subscribeMethod);
    }
}