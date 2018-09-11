using Application.Framework;
using Domain.Framework;

namespace Adapters.Json.ObjectPersistences
{
    public class DomainEventConverter : JSonConverter<DomainEvent>
    {
    }

    public class QuerryConverter : JSonConverter<Query>
    {
    }
}