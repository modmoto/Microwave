using Microwave.Discovery.EventLocations;

namespace Microwave.Persistence.MongoDb.Querries
{
    public interface IEventLocationCache
    {
        bool HasValue { get; }
        void Update(EventLocation eventLocation);
        EventLocation GetValue();
    }
}