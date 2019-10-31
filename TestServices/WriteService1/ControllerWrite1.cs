using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microwave.Domain.Results;
using Microwave.EventStores;

namespace WriteService1
{
    [Route("api/write1")]
    public class ControllerWrite1 : Controller
    {
        private readonly IEventStore _eventStore;
        private readonly string _entityId = "1FFE7557-CAC4-4904-84D1-0FD561453DE7";

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

        [HttpPost("{guidId}")]
        public ActionResult WriteEvent(Guid guidId)
        {
            return Ok();
        }

        [HttpPost("{guidId}/tests")]
        public ActionResult WriteEventSecond(Guid guidId)
        {
            return Ok();
        }
    }
}