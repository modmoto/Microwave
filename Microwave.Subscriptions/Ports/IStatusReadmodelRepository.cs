using System.Threading.Tasks;
using Microwave.Subscriptions.ReadModels;

namespace Microwave.Subscriptions.Ports
{
    public interface IStatusReadmodelRepository
    {
        Task<EventLocationReadModel> GetEventLocation();
    }
}