using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microwave.Subscriptions;

namespace Microwave.WebApi.Discovery
{
    [Route("Subscriptions")]
    public class SubscriptionController : Controller
    {
        private readonly ISubscriptionHandler _subscriptionHandler;

        public SubscriptionController(
            ISubscriptionHandler subscriptionHandler)
        {
            _subscriptionHandler = subscriptionHandler;
        }

        [HttpPost("Subscriptions")]
        public async Task<ActionResult> SubscribeEvent([FromBody] Subscription subscribeEventDto)
        {
            await _subscriptionHandler.StoreSubscription(subscribeEventDto);
            return Ok();
        }

        [HttpPost("Subscriptions/{eventType}")]
        public async Task<ActionResult> SubscribeEvent(string eventType, [FromBody] StoreNewVersionCommand command)
        {
            command.EventType = eventType;
            await _subscriptionHandler.StoreNewRemoteVersion(command);
            return Ok();
        }

        [HttpPost("Subscriptions/Overall")]
        public async Task<ActionResult> SubscribeEvent([FromBody] StoreNewOverallVersionCommand command)
        {
            await _subscriptionHandler.StoreNewRemoteOverallVersion(command);
            return Ok();
        }

        [HttpGet("Subscriptions")]
        public async Task<ActionResult> GetSubscriptions()
        {
            var res = await _subscriptionHandler.GetSubscriptions();
            return Ok(res);
        }
    }
}