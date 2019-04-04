using Microwave.Application.Discovery;

namespace Microwave.Application
{
    public class MicrowaveConfiguration
    {
        public string ServiceName { get; set; }
        public DatabaseConfig DatabaseConfigDatabase { get; set; } = new DatabaseConfig(); 
        public ServiceBaseAddressCollection ServiceLocations { get; set; } = new ServiceBaseAddressCollection();

    }

    public class DatabaseConfig
    {
        public string ConnectionString { get; set; } = "mongodb://localhost:27017/";
        public string DatabaseName { get; set; } = "MicrowaveDb";
    }
}