using System;
using System.Linq;
using Microwave.Queries.Polling;

namespace Microwave.UI.Areas.MicrowaveDashboard.Pages
{
    public class JobDto
    {
        public int Index { get; }
        public string GenericType { get; }
        public string HandlerName { get; }
        public string EventName { get; }
        public DateTime NextRun { get; }

        public JobDto(IMicrowaveBackgroundService hostedService, int index)
        {
            Index = index;
            GenericType = hostedService.GetType().GetGenericArguments().First().GetGenericTypeDefinition().Name;
            var genericHandler = hostedService.GetType().GetGenericArguments().First();
            HandlerName = genericHandler.GetGenericArguments().FirstOrDefault()?.Name;
            EventName = genericHandler.GetGenericArguments().LastOrDefault()?.Name;
            NextRun = hostedService.NextRun;
        }
    }
}