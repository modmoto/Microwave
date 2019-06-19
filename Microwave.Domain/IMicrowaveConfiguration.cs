namespace Microwave.Domain
{
    public interface IMicrowaveConfiguration
    {
        string ServiceName { get; set; }
        IDatabaseConfiguration DatabaseConfiguration { get; set; }
        IServiceBaseAddressCollection ServiceLocations { get; set; }
    }
}