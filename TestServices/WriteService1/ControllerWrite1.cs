using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microwave.Domain.Identities;
using Microwave.Domain.Results;
using Microwave.EventStores;

namespace WriteService1
{
    [Route("api/write1")]
    public class ControllerWrite1 : Controller
    {
        private readonly IEventStore _eventStore;
        private readonly Identity _entityId = Identity.Create("1FFE7557-CAC4-4904-84D1-0FD561453DE7");

        public ControllerWrite1(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        [HttpPost("")]
        public async Task<ActionResult> WriteEvent()
        {
            var appendAsync = await _eventStore.LoadAsync<EntityTest>(_entityId);
            if (appendAsync.Is<NotFound>())
            {
                (await _eventStore.AppendAsync(new Event2(_entityId, "jeah"), 0)).Check();
            }
            else
            {
                (await _eventStore.AppendAsync(new Event2(_entityId, "jeah"), appendAsync.Version)).Check();
            }
            return Ok();
        }
    }
}