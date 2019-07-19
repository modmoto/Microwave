namespace Microwave.Domain
{
    public interface IDatabaseConfiguration
    {
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
        string PrimaryKey { get; set; }
    }
}