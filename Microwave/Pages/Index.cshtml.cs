using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microwave.Application.Discovery;

namespace Microwave.Pages
{
    public class IndexModel : PageModel
    {
        private readonly DiscoveryHandler _discoveryHandler;

        public IEventLocation ConsumingServices { get; set; }

        public bool HasMissingEvents => ConsumingServices.UnresolvedEventSubscriptions.Any()
                                        || ConsumingServices.UnresolvedReadModeSubscriptions.Any();

        public IndexModel(
            DiscoveryHandler discoveryHandler)
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
            return Redirect("#");
        }
    }
}