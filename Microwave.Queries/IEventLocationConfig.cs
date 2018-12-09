using System;

namespace Microwave.Queries
{
    public interface IEventLocationConfig
    {
        Uri GetLocationForDomainEvent(string domainEvent);
        Uri GetLocationForReadModel(string name);
    }
}