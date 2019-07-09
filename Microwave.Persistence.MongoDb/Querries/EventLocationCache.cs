using Microwave.Discovery.EventLocations;

namespace Microwave.Persistence.MongoDb.Querries
{
    public class EventLocationCache
    {
        private IEventLocation _eventLocation;
        public bool HasValue => _eventLocation != null;

        public void Update(IEventLocation eventLocation)
        {
            _eventLocation = eventLocation;
        }

        public IEventLocation GetValue()
        {
            return _eventLocation;
        }
    }
}