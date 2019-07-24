using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microwave.Discovery;
using Microwave.Discovery.EventLocations;

namespace Microwave.UI.Areas.MicrowaveDashboard.Pages
{
    public class IndexModel : MicrowavePageModel
    {
        private readonly IDiscoveryHandler _discoveryHandler;

        public EventLocation ConsumingServices { get; set; }

        public bool HasMissingEvents => ConsumingServices.UnresolvedEventSubscriptions.Any()
                                        || ConsumingServices.UnresolvedReadModeSubscriptions.Any();

        public IndexModel(
            IDiscoveryHandler discoveryHandler,
            MicrowaveConfiguration configuration) : base(configuration)
        {
            _discoveryHandler = discoveryHandler;
        }

        public async Task OnGetAsync()
        {
            var consumingServices = await _discoveryHandler.GetConsumingServices();
            ConsumingServices = consumingServices;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _discoveryHandler.DiscoverConsumingServices();
            return Redirect("/MicrowaveDashboard");
        }
    }
}