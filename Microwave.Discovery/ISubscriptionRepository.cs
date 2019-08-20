using System.Threading.Tasks;
using Microwave.Discovery.EventLocations;
using Microwave.Discovery.ServiceMaps;

namespace Microwave.Discovery
{
    public interface ISubscriptionRepository
    {
        Task SubscribeForEvent(MicrowaveServiceNode microwaveServiceNode, EventSchema subscribedEvent);
    }
}