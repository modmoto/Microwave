using Microwave.Discovery.EventLocations;

namespace Microwave.Persistence.MongoDb.Querries
{
    internal class EventLocationCache : IEventLocationCache
    {
        private EventLocation _eventLocation;
        public bool HasValue => _eventLocation != null;

        public void Update(EventLocation eventLocation)
        {
            _eventLocation = eventLocation;
        }

        public EventLocation GetValue()
        {
            return _eventLocation;
        }
    }
}