using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Microwave.WebApi")]
[assembly: InternalsVisibleTo("Microwave.Persistence.MongoDb")]
[assembly: InternalsVisibleTo("Microwave.Queries.UnitTests")]
[assembly: InternalsVisibleTo("Microwave.Persistence.UnitTests")]
[assembly: InternalsVisibleTo("Microwave.Persistence.UnitTestSetupPorts")]
[assembly: InternalsVisibleTo("Microwave.Persistence.MongoDb.UnitTestsSetup")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace Microwave.Queries.Ports
{
    internal interface IEventFeed<T>
    {
        Task<IEnumerable<SubscribedDomainEventWrapper>> GetEventsAsync(DateTimeOffset since = default(DateTimeOffset));
    }
}