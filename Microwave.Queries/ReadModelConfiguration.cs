using Microwave.Application.Discovery;

namespace Microwave.Queries
{
    public class ReadModelConfiguration
    {
        public ReadDatabaseConfig Database { get; set; } = new ReadDatabaseConfig();
        public ServiceBaseAddressCollection ServiceLocations { get; set; } = new ServiceBaseAddressCollection();
    }
}