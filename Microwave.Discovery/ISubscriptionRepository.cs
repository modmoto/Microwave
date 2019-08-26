using System.Threading.Tasks;

namespace Microwave.Discovery
{
    public interface ISubscriptionRepository
    {
        Task SubscribeForEvent(Subscription subscription);
    }
}