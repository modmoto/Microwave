using Microwave.Discovery.Domain;

namespace Microwave.Queries.Persistence.MongoDb
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