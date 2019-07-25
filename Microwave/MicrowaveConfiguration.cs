using System.Collections.Generic;
using Microwave.EventStores.SnapShots;
using Microwave.Queries.Polling;
using Microwave.WebApi;

namespace Microwave
{
    public class MicrowaveConfiguration
    {
        public string ServiceName { get; set; }
        public ServiceBaseAddressCollection ServiceLocations { get; set; } = new ServiceBaseAddressCollection();
        public IMicrowaveHttpClientCreator MicrowaveHttpClientCreator { get; set; } = new DefaultMicrowaveHttpClientCreator();
        public IEnumerable<ISnapShot> SnapShotConfigurations { get; set; } = new List<ISnapShot>();
        public IEnumerable<IPollingInterval> UpdateEveryConfigurations { get; set; } = new List<IPollingInterval>();
    }
}