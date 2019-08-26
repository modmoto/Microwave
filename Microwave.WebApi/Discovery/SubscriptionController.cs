using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microwave.Discovery;

namespace Microwave.WebApi.Discovery
{
    [Route("Dicovery")]
    public class SubscriptionController : Controller
    {
        private readonly ISubscriptionHandler _subscriptionHandler;

        public SubscriptionController(ISubscriptionHandler subscriptionHandler)
        {
            _subscriptionHandler = subscriptionHandler;
        }

        [HttpPost("Subscriptions")]
        public async Task<ActionResult> SubscribeEvent([FromBody] Subscription subscribeEventDto)
        {
            await _subscriptionHandler.SubscribeForEvent(subscribeEventDto);
            return Ok();
        }
    }
}