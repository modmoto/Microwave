using System;

namespace Microwave.Queries
{
    public interface IEventLocationConfig
    {
        Uri GetLocationFor(string domainEvent);
    }
}