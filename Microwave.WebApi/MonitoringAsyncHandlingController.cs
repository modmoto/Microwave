using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microwave.Queries;

namespace Microwave.WebApi
{
    [Route("Monitoring")]
    public class MonitoringAsyncHandlingController : Controller
    {
        private readonly IVersionRepository _versionRepository;

        public MonitoringAsyncHandlingController(IVersionRepository versionRepository)
        {
            _versionRepository = versionRepository;
        }

        [HttpGet("ProcessedVersions/{eventType}")]
        public async Task<ActionResult> GetProcessedEventVersions(string eventType)
        {
            var dateTimeOffset = await _versionRepository.GetVersionAsync(eventType);
            return Ok(dateTimeOffset);
        }
    }
}