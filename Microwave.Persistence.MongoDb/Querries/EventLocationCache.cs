using Microwave.Discovery.EventLocations;

namespace Microwave.Persistence.MongoDb.Querries
{
    public class EventLocationCache
    {
        private EventLocation _eventLocation;
        public bool HasValue => _eventLocation != null;

        public void Update(EventLocation eventLocation)
        {
            _eventLocation = eventLocation;
        }

        public IEventLocation GetValue()
        {
            return _eventLocation;
        }
    }
}