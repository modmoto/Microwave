using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microwave.Discovery;
using Microwave.Discovery.Domain.Services;
using Microwave.Domain;

namespace Microwave.UI.Areas.MicrowaveDashboard.Pages
{
    public class ServiceMapPage : MicrowavePageModel
    {
        private readonly IDiscoveryHandler _handler;
        private ServiceMap _serviceMap;

        public IEnumerable<ServiceNodeConfig> ReachableServices =>
            _serviceMap.AllServices.Where(s => s.IsReachable);
        public IEnumerable<ServiceNodeConfig> UnreachableServices =>
            _serviceMap.AllServices.Where(s => !s.IsReachable);
        public bool MapIsDiscovered => _serviceMap != null;

        private IEnumerable<ServiceNodeConfig> ServicesSortedByIncomingNodes
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

        public IEnumerable<Node> OrderedNodes
        {
            get
            {
                var serviceNodeConfigs = ServicesSortedByIncomingNodes.ToList();
                var edgeCount = Math.Ceiling(Math.Sqrt(serviceNodeConfigs.Count));
                var nodes = new List<Node>();
                var counter = 0;
                for (int j = 0; j < edgeCount; j++)
                {
                    for (int k = 0; k < edgeCount; k++)
                    {
                        if (serviceNodeConfigs.Count <= counter) continue;
                        var serviceNodeConfig = serviceNodeConfigs[counter];
                        nodes.Add(new Node(serviceNodeConfig.ServiceEndPoint.Name,
                            serviceNodeConfig.ServiceEndPoint.ServiceBaseAddress, k, j));
                        counter++;
                    }
                }

                return nodes;
            }
        }
        public IEnumerable<Edge> Edges
        {
            get
            {
                var serviceNodeConfigs = ReachableServices.ToList();
                var edges = new List<Edge>();
                foreach (var serviceNodeConfig in serviceNodeConfigs)
                {
                    foreach (var serviceEndPoint in serviceNodeConfig.Services)
                    {
                        edges.Add(new Edge(serviceNodeConfig.ServiceEndPoint.ServiceBaseAddress, serviceEndPoint.ServiceBaseAddress));
                    }
                }

                return edges;
            }
        }

        public ServiceMapPage(
            IDiscoveryHandler handler,
            MicrowaveConfiguration configuration) : base(configuration)
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

    public class Edge
    {
        public Edge(Uri source, Uri target)
        {
            id = $"{source}_{target}";
            this.source = source.ToString();
            this.target = target.ToString();
        }

        public string id { get; }
        public string source { get; }
        public string target { get; }
        public int size => 1;
        public string color => "#ccc";
    }

    public class Node
    {
        public Node(string label, Uri serviceAddress, int x, int y)
        {
            id = serviceAddress.ToString();
            this.label = label;
            this.x = x;
            this.y = y;
            this.serviceAddress = serviceAddress.ToString();
        }

        public string id { get; }
        public string label { get; }
        public int x { get; }
        public int y { get; }
        public string serviceAddress { get; }
        public int size => 1;
    }
}