using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microwave.Queries.Polling;
using Microwave.WebApi;

namespace Microwave.UI.Areas.MicrowaveDashboard.Pages
{
    public class JobsPage : MicrowavePageModel
    {
        public JobsPage(MicrowaveWebApiConfiguration configuration, IEnumerable<IHostedService> jobs) : base
        (configuration)
        {
            Jobs = jobs.Where(j => j.GetType().IsGenericType
                                   && j.GetType().GetGenericTypeDefinition() == typeof(MicrowaveBackgroundService<>)
                                   && j.GetType().GetGenericArguments().First() != typeof(DiscoveryPoller)).Select(j
                                   => new JobDto(j));
        }

        public async Task<IActionResult> OnPostAsync()
        {
            return Redirect("JobsPage");
        }
        public IEnumerable<JobDto> Jobs { get; }
    }

    public class JobDto
    {
        public string GenericType { get; }
        public string HandlerName { get; }
        public string EventName { get; }

        public JobDto(IHostedService hostedService)
        {
            GenericType = hostedService.GetType().GetGenericArguments().First().GetGenericTypeDefinition().Name;
            var genericHandler = hostedService.GetType().GetGenericArguments().First();
            HandlerName = genericHandler.GetGenericArguments().FirstOrDefault()?.Name;
            EventName = genericHandler.GetGenericArguments().LastOrDefault()?.Name;
        }
    }
}