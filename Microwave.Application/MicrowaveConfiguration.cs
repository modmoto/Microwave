namespace Microwave.Application
{
    public class MicrowaveConfiguration
    {
        public string ServiceName { get; set; }
        public DatabaseConfiguration DatabaseConfiguration { get; set; } = new DatabaseConfiguration(); 
        public ServiceBaseAddressCollection ServiceLocations { get; set; } = new ServiceBaseAddressCollection();

    }

    public class DatabaseConfiguration
    {
        public string ConnectionString { get; set; } = "mongodb://localhost:27017/";
        public string DatabaseName { get; set; } = "MicrowaveDb";
    }
}