using System.Threading.Tasks;

namespace Microwave.Application.Monitoring
{
    public class MonitoringHandler
    {
        private readonly IEventRepository _eventRepository;

        public MonitoringHandler(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }
        public async Task<long> GetEventTypeCount(string eventType)
        {
            var result = await _eventRepository.GetEventTypeCount(eventType);
            return result.Value;
        }
    }
}