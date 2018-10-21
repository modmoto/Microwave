using System.Threading.Tasks;
using Application.Framework;
using Microsoft.AspNetCore.Mvc;

namespace Adapters.WebApi.Seasons
{
    [Route("Api/StartHandler")]
    public class StartEventUpdateController : Controller
    {
        private readonly AsyncEventDelegator _delegator;

        public StartEventUpdateController(AsyncEventDelegator delegator)
        {
            _delegator = delegator;
        }

        [HttpPost]
        public async Task DoHandle()
        {
            await _delegator.Update();
        }
    }
}