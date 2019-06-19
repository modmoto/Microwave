using Microwave.Domain;

namespace Microwave
{
    public class MicrowaveConfiguration : IMicrowaveConfiguration
    {
        public string ServiceName { get; set; }
        public IDatabaseConfiguration DatabaseConfiguration { get; set; } = new DatabaseConfiguration();
        public IServiceBaseAddressCollection ServiceLocations { get; set; } = new ServiceBaseAddressCollection();
    }
}