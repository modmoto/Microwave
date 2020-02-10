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
        private readonly IEnumerable<IMicrowaveBackgroundService> _jobs;

        public JobsPage(MicrowaveWebApiConfiguration configuration, IEnumerable<IMicrowaveBackgroundService> jobs) : base
        (configuration)
        {
            _jobs = jobs;
        }

        public async Task<IActionResult> OnPostAsync(int index)
        {
            var hostedService = _jobs.ToList()[index];
            await hostedService.RunAsync();
            return Redirect("JobsPage");
        }

        public IEnumerable<JobDto> Jobs => _jobs.Where(j => j.GetType().GetGenericArguments().First() !=
                                                            typeof(DiscoveryPoller)).Select((j, index) => new JobDto
                                                            (j, index));
    }
}