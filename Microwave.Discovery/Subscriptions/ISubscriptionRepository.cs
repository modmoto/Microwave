using System.Threading.Tasks;

namespace Microwave.Discovery.Subscriptions
{
    public interface ISubscriptionRepository
    {
        Task SubscribeForEvent(Subscription subscription);
    }
}