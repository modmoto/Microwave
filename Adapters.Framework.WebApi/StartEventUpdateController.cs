using System.Threading.Tasks;
using Application.Framework;
using Application.Seasons.Querries;
using Microsoft.AspNetCore.Mvc;

namespace Adapters.Framework.WebApi
{
    [Route("Api/StartHandler")]
    public class StartEventUpdateController : Controller
    {
        private readonly AsyncEventDelegator _delegator;
        private readonly IQeryRepository _qeryRepository;

        public StartEventUpdateController(AsyncEventDelegator delegator, IQeryRepository qeryRepository)
        {
            _delegator = delegator;
            _qeryRepository = qeryRepository;
        }

        [HttpPost]
        public async Task DoHandle()
        {
            await _delegator.Update();
        }

        [HttpPost("saveQ")]
        public async Task DoSaveQUerru()
        {
            var allSeasonsQuery = await _qeryRepository.Load<AllSeasonsQuery>();
            if (allSeasonsQuery != null)
            {
                allSeasonsQuery.Value.Seasons.Add(new SeasonDto());
                await _qeryRepository.Save(allSeasonsQuery.Value);
            }
            else
            {
                await _qeryRepository.Save(new AllSeasonsQuery());
            }
        }
    }
}