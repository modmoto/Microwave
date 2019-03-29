using Microwave.Application.Discovery;

namespace Microwave.Application
{
    public class MicrowaveConfiguration
    {
        public WriteDatabaseConfig WriteDatabase { get; set; } = new WriteDatabaseConfig();
        public ReadDatabaseConfig ReadDatabase { get; set; } = new ReadDatabaseConfig();
        public ServiceBaseAddressCollection ServiceLocations { get; set; } = new ServiceBaseAddressCollection();
    }

    public class WriteDatabaseConfig
    {
        public string ConnectionString { get; set; } = "mongodb://localhost:27017/";
        public string DatabaseName { get; set; } = "MicrowaveWriteModelDb";
    }

    public class ReadDatabaseConfig
    {
        public string ConnectionString { get; set; } = "mongodb://localhost:27017/";
        public string DatabaseName { get; set; } = "MicrowaveReadModelDb";
    }
}