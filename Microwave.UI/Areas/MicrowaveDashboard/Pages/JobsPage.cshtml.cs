using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microwave.Queries.Handler;
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

        public IEnumerable<JobDto> AsyncHandlers => Jobs.Where(j => j.GenericType == GetNameOfType(typeof(AsyncEventHandler<,>)));
        public IEnumerable<JobDto> QueryHandlers => Jobs.Where(j => j.GenericType == GetNameOfType(typeof(QueryEventHandler<,>)));
        public IEnumerable<JobDto> ReadModelHandlers => Jobs.Where(j => j.GenericType == GetNameOfType(typeof(ReadModelEventHandler<>)));

        public IEnumerable<JobDto> Jobs => _jobs.Where(j =>
        {
            var jobType = j.GetType();
            var memberInfo = jobType.GetGenericArguments().First();
            return memberInfo != typeof(DiscoveryPoller);
        }).Select((j, index) => new JobDto(j, index));

        private static string GetNameOfType(Type t)
        {
            return t.Name.Substring(0, t.Name.Length - 2);
        }
    }
}