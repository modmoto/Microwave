using Microwave.Discovery;

namespace Microwave.WebApi
{
    public class MicrowaveWebApiConfiguration
    {
        public string ServiceName { get; private set; }
        public ServiceBaseAddressCollection ServiceLocations { get; private set; } = new ServiceBaseAddressCollection();
        public IMicrowaveHttpClientFactory MicrowaveHttpClientFactory { get; private set; } = new DefaultMicrowaveHttpClientFactory();

        public void WithHttpClientFactory(IMicrowaveHttpClientFactory clientFactory)
        {
            MicrowaveHttpClientFactory = clientFactory;
        }

        public void WithServiceName(string serviceName)
        {
            ServiceName = serviceName;
        }

        public void WithServiceLocations(ServiceBaseAddressCollection serviceBaseAddressCollection)
        {
            ServiceLocations = serviceBaseAddressCollection;
        }
    }
}