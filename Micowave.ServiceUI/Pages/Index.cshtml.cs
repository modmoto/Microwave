using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microwave.Application.Discovery;

namespace Micowave.ServiceUI.Pages
{
    public class IndexModel : PageModel
    {
        private readonly DiscoveryHandler _discoveryHandler;

        public IEventLocation ConsumingServices { get; set; }

        public IndexModel(DiscoveryHandler discoveryHandler)
        {
            _discoveryHandler = discoveryHandler;
        }

        public void OnGet()
        {
            var consumingServices = _discoveryHandler.GetConsumingServices();
            ConsumingServices = consumingServices;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _discoveryHandler.DiscoverConsumingServices();
            return Redirect("#");
        }
    }
}