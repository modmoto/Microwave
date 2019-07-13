using Microwave.WebApi;

namespace Microwave
{
    public class MicrowaveConfiguration
    {
        public string ServiceName { get; set; }
        public ServiceBaseAddressCollection ServiceLocations { get; set; } = new ServiceBaseAddressCollection();
        public IMicrowaveHttpClientCreator MicrowaveHttpClientCreator { get; set; } = new DefaultMicrowaveHttpClientCreator();
    }
}