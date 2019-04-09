using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microwave.Discovery
{
    public interface IServiceDiscoveryRepository
    {
        Task<EventsPublishedByService> GetPublishedEventTypes(Uri serviceAdress);
        Task<ServiceNode> GetDependantServices(Uri serviceAdress);
    }

    public class ServiceNode
    {
        public static ServiceNode Ok(NodeEntryPoint nodeEntryPoint, IEnumerable<MicrowaveService> services)
        {
            return new ServiceNode(nodeEntryPoint, true, services);
        }

        public static ServiceNode NotReachable(NodeEntryPoint nodeEntryPoint)
        {
            return new ServiceNode(nodeEntryPoint, true, new List<MicrowaveService>());
        }

        private ServiceNode(NodeEntryPoint nodeEntryPoint, bool isReachable, IEnumerable<MicrowaveService> services)
        {
            NodeEntryPoint = nodeEntryPoint;
            IsReachable = isReachable;
            Services = services;
        }

        public NodeEntryPoint NodeEntryPoint { get; set; }
        public bool IsReachable { get; set; }
        public IEnumerable<MicrowaveService> Services { get; set; }
    }

    public class NodeEntryPoint
    {
        public NodeEntryPoint(Uri serviceBaseAddress, string name = null)
        {
            Name = name ?? serviceBaseAddress.ToString();
            ServiceBaseAddress = serviceBaseAddress;
        }

        public string Name { get; }
        public Uri ServiceBaseAddress { get; }
    }
}