using System;

namespace Microwave.Application.Discovery
{
    public interface IEventLocation
    {
        ConsumingService GetService(Type eventType);
        void SetDomainEventLocation(ConsumingService service);
    }
}