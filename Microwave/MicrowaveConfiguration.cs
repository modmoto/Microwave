using System.Collections.Generic;
using Microwave.Domain.EventSourcing;
using Microwave.Queries;
using Microwave.WebApi;

namespace Microwave
{
    public class MicrowaveConfiguration
    {
        public string ServiceName { get; set; }
        public ServiceBaseAddressCollection ServiceLocations { get; set; } = new ServiceBaseAddressCollection();
        public IMicrowaveHttpClientCreator MicrowaveHttpClientCreator { get; set; } = new DefaultMicrowaveHttpClientCreator();
        public IEnumerable<ISnapShotAfter> SnapShotConfigurations { get; set; } = new List<ISnapShotAfter>();
        public IEnumerable<IUpdateEveryConfig> UpdateEveryConfigurations { get; set; } = new List<IUpdateEveryConfig>();
    }
}