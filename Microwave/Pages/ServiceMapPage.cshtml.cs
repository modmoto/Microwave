using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microwave.Discovery;

namespace Microwave.Pages
{
    public class ServiceMapPage : PageModel
    {
        private readonly DiscoveryHandler _handler;

        public ServiceMap ServiceMap { get; private set; }

        public ServiceMapPage(DiscoveryHandler handler)
        {
            _handler = handler;
        }

        public async Task OnGetAsync()
        {
            var serviceMap = await _handler.GetServiceMap();
            ServiceMap = serviceMap;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _handler.DiscoverServiceMap();
            return Redirect("ServiceMapPage");
        }
    }
}