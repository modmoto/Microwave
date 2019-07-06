namespace Microwave.Domain
{
    public interface IMicrowaveConfiguration
    {
        string ServiceName { get; }
        IServiceBaseAddressCollection ServiceLocations { get; }
    }
}