using System.Threading.Tasks;
using Microwave.Discovery;
using Microwave.Discovery.EventLocations;
using Microwave.Discovery.ServiceMaps;

namespace Microwave.WebApi.Discovery
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
        public Task SubscribeForEvent(MicrowaveServiceNode microwaveServiceNode, EventSchema subscribedEvent)
        {
            throw new System.NotImplementedException();
        }

        public Task SubscribeForReadModel(MicrowaveServiceNode microwaveServiceNode, ReadModelSubscription subscribedReadModel)
        {
            throw new System.NotImplementedException();
        }
    }
}