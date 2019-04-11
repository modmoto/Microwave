using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microwave.Discovery;
using Microwave.Discovery.Domain.Services;

namespace Microwave.Pages
{
    public class ServiceMapPage : PageModel
    {
        private readonly DiscoveryHandler _handler;
        private ServiceMap _serviceMap;

        public IEnumerable<ServiceNodeConfig> ReachableServices =>
            _serviceMap.AllServices.Where(s => s.IsReachable);
        public IEnumerable<ServiceNodeConfig> UnreachableServices =>
            _serviceMap.AllServices.Where(s => !s.IsReachable);
        public bool MapIsDiscovered => _serviceMap != null;

        public ServiceMapPage(DiscoveryHandler handler)
        {
            _handler = handler;
        }

        public async Task OnGetAsync()
        {
            var serviceMap = await _handler.GetServiceMap();
            _serviceMap = serviceMap;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _handler.DiscoverServiceMap();
            return Redirect("ServiceMapPage");
        }
    }
}