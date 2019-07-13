using Microwave.Discovery;
using Microwave.Domain;

namespace Microwave
{
    public class MicrowaveConfiguration : IMicrowaveConfiguration
    {
        public string ServiceName { get; set; }
        public IServiceBaseAddressCollection ServiceLocations { get; set; } = new ServiceBaseAddressCollection();
        public IMicrowaveHttpClientCreator MicrowaveHttpClientCreator { get; set; } = new DefaultMicrowaveHttpClientCreator();
    }
}