﻿using System.Linq;
using System.Threading.Tasks;
using Microwave.Discovery;
using Microwave.Discovery.EventLocations;

namespace Microwave.UI.Areas.MicrowaveDashboard.Pages
{
    public class IndexModel : MicrowavePageModel
    {
        private readonly IDiscoveryHandler _discoveryHandler;
        public EventLocation ConsumingServices { get; set; }
        public EventsPublishedByService PublishedEvents { get; set; }

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
            var publishedEvents = await _discoveryHandler.GetPublishedEvents();
            ConsumingServices = consumingServices;
            PublishedEvents = publishedEvents;
        }

        public async Task OnPostAsync()
        {
            await _discoveryHandler.DiscoverConsumingServices();
            Redirect("/MicrowaveDashboard");
        }
    }
}