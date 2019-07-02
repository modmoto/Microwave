namespace Microwave.Domain
{
    public interface IMicrowaveConfiguration
    {
        string ServiceName { get; }
        IDatabaseConfiguration DatabaseConfiguration { get; }
        IServiceBaseAddressCollection ServiceLocations { get; }
    }
}