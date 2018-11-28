using System;

namespace Microwave.Application
{
    public interface IEventLocationConfig
    {
        Uri GetLocationFor(string domainEvent);
    }
}