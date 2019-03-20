using System;

namespace Microwave.Application.Discovery
{
    public interface IEventLocation
    {
        Uri GetDomainEventLocation(Type eventType);
        ConsumingService GetService(Type eventType);
        void SetDomainEventLocation(ConsumingService service);
    }
}