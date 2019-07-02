namespace Microwave.Domain
{
    public interface IDatabaseConfiguration
    {
        string ConnectionString { get; }
        string DatabaseName { get; }
    }
}