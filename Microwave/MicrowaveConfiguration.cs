using Microwave.Domain;
using Microwave.WebApi;

namespace Microwave
{
    public class MicrowaveConfiguration
    {
        public string ServiceName { get; set; }
        public IServiceBaseAddressCollection ServiceLocations { get; set; } = new ServiceBaseAddressCollection();
        public IMicrowaveHttpClientCreator MicrowaveHttpClientCreator { get; set; } = new DefaultMicrowaveHttpClientCreator();
    }
}