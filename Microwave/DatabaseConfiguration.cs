using Microwave.Domain;

namespace Microwave
{
    public class DatabaseConfiguration : IDatabaseConfiguration
    {
        public string ConnectionString { get; set; } = "mongodb://localhost:27017/";
        public string DatabaseName { get; set; } = "MicrowaveDb";
        public string PrimaryKey { get; set; }
    }
}