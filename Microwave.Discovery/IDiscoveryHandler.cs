using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microwave.Discovery.EventLocations;
using Microwave.Discovery.ServiceMaps;

[assembly: InternalsVisibleTo("Microwave")]
[assembly: InternalsVisibleTo("Microwave.WebApi.UnitTests")]
[assembly: InternalsVisibleTo("Microwave.UnitTests")]
[assembly: InternalsVisibleTo("Microwave.UI.UnitTests")]
[assembly: InternalsVisibleTo("Microwave.Persistence.UnitTestSetupPorts")]
[assembly: InternalsVisibleTo("Microwave.Persistence.UnitTests")]
[assembly: InternalsVisibleTo("Microwave.Persistence.MongoDb.UnitTestsSetup")]
[assembly: InternalsVisibleTo("Microwave.Discovery.UnitTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace Microwave.Discovery
{
    public interface IDiscoveryHandler
    {
        Task<EventLocation> GetConsumingServices();
        Task<ServiceMap> GetServiceMap();
        Task DiscoverConsumingServices();
        Task DiscoverServiceMap();
        Task<MicrowaveServiceNode> GetConsumingServiceNodes();
        Task<EventsPublishedByService> GetPublishedEvents();
    }
}