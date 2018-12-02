using System;

namespace Microwave.Application.Ports
{
    public interface IEventLocationConfig
    {
        Uri GetLocationFor(string domainEvent);
    }
}