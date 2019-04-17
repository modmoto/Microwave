using System;
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
        private readonly IDiscoveryHandler _handler;
        private ServiceMap _serviceMap;

        public IEnumerable<ServiceNodeConfig> ReachableServices =>
            _serviceMap.AllServices.Where(s => s.IsReachable);
        public IEnumerable<ServiceNodeConfig> UnreachableServices =>
            _serviceMap.AllServices.Where(s => !s.IsReachable);
        public IEnumerable<ServiceNodeConfig> ServicesSortedByIncomingNodes
        {
            get
            {
                var serviceDependencyCounter = new Dictionary<Uri, int>();
                foreach (var reachableService in ReachableServices)
                {
                    var serviceBaseAddress = reachableService.ServiceEndPoint.ServiceBaseAddress;
                    var serviceNodeConfigs = ReachableServices.SelectMany(s => s.Services);
                    var serviceDependencies = serviceNodeConfigs.Count(s => s.ServiceBaseAddress == serviceBaseAddress);
                    serviceDependencyCounter.Add(serviceBaseAddress, serviceDependencies);
                }

                var sortedList = serviceDependencyCounter.ToList();
                sortedList.Sort((x, y) => y.Value - x.Value);

                var nodeConfigs = new List<ServiceNodeConfig>();
                foreach (var item in sortedList)
                {
                    nodeConfigs.Add(ReachableServices.Single(r => r.ServiceEndPoint.ServiceBaseAddress == item.Key));
                }

                return nodeConfigs;
            }
        }

        public bool MapIsDiscovered => _serviceMap != null;

        public ServiceMapPage(IDiscoveryHandler handler)
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