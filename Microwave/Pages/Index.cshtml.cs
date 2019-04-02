using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microwave.Application.Discovery;
using Microwave.EventStores.Ports;
using Microwave.Queries;

namespace Microwave.Pages
{
    public class IndexModel : PageModel
    {
        private readonly DiscoveryHandler _discoveryHandler;
        private readonly IEventRepository _eventRepo;
        private readonly IVersionRepository _versionRepository;

        public IEventLocation ConsumingServices { get; set; }
        public IndexModel(
            DiscoveryHandler discoveryHandler,
            IEventRepository eventRepo,
            IVersionRepository versionRepository)
        {
            _discoveryHandler = discoveryHandler;
            _eventRepo = eventRepo;
            _versionRepository = versionRepository;
        }

        public void OnGet()
        {
            var consumingServices = _discoveryHandler.GetConsumingServices();
            ConsumingServices = consumingServices;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _discoveryHandler.DiscoverConsumingServices();
            var consumingServices = _discoveryHandler.GetConsumingServices();
            foreach (var VARIABLE in consumingServices.Services)
            {

            }
            return Redirect("#");
        }
    }
}