using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microwave.Queries.Polling;
using Microwave.WebApi;

namespace Microwave.UI.Areas.MicrowaveDashboard.Pages
{
    public class JobsPage : MicrowavePageModel
    {
        private readonly IEnumerable<IHostedService> _jobs;

        public JobsPage(MicrowaveWebApiConfiguration configuration, IEnumerable<IHostedService> jobs) : base
        (configuration)
        {
            _jobs = jobs;
        }

        public async Task<IActionResult> OnPostAsync(int index)
        {
            var hostedService = _jobs.ToList()[index];
            await hostedService.StopAsync(CancellationToken.None);
            await hostedService.StartAsync(CancellationToken.None);
            return Redirect("JobsPage");
        }

        public IEnumerable<JobDto> Jobs => _jobs.Where(j => j.GetType().IsGenericType
                                                            && j.GetType().GetGenericTypeDefinition() ==
                                                            typeof(MicrowaveBackgroundService<>)
                                                            && j.GetType().GetGenericArguments().First() !=
                                                            typeof(DiscoveryPoller)).Select((j, index) => new JobDto
                                                            (j, index));
    }

    public class JobDto
    {
        public int Index { get; }
        public string GenericType { get; }
        public string HandlerName { get; }
        public string EventName { get; }

        public JobDto(IHostedService hostedService, int index)
        {
            Index = index;
            GenericType = hostedService.GetType().GetGenericArguments().First().GetGenericTypeDefinition().Name;
            var genericHandler = hostedService.GetType().GetGenericArguments().First();
            HandlerName = genericHandler.GetGenericArguments().FirstOrDefault()?.Name;
            EventName = genericHandler.GetGenericArguments().LastOrDefault()?.Name;
        }
    }
}