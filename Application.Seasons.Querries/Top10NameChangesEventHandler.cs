using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Framework;
using Application.Framework.Results;
using Domain.Seasons.Events;

namespace Application.Seasons.Querries
{
    public class Top10NameChangesEventHandler : IHandleAsync<SeasonCreatedEvent>, IHandleAsync<SeasonNameChangedEvent>
    {
        private readonly IQeryRepository _qeryRepository;
        private readonly IEntityStreamRepository _streamRepository;

        public Top10NameChangesEventHandler(IQeryRepository qeryRepository, IEntityStreamRepository streamRepository)
        {
            _qeryRepository = qeryRepository;
            _streamRepository = streamRepository;
        }

        public async Task HandleAsync(SeasonCreatedEvent domainEvent)
        {
            var result = await _qeryRepository.Load<Top10NameChangers>();
            if (result.Is<NotFound<Top10NameChangers>>()) result = Result<Top10NameChangers>.Ok(new Top10NameChangers());

            var top10NameChangers = result.Value;
            var seasonNameChangedEvents = SortIn(top10NameChangers.SeasonCounter, new SeasonNameChangCounterDto(domainEvent.EntityId, 0));
            top10NameChangers.SeasonCounter = seasonNameChangedEvents;

            await _qeryRepository.Save(top10NameChangers);
        }

        private IEnumerable<SeasonNameChangCounterDto> SortIn(IEnumerable<SeasonNameChangCounterDto> valueSeasonCounter, SeasonNameChangCounterDto seasonNameChangCounterDto)
        {
            var nameChangCounterDtos = valueSeasonCounter.ToList();
            var nameChangCounterDto = nameChangCounterDtos.FirstOrDefault(e => e.SeasonId == seasonNameChangCounterDto.SeasonId);
            if (nameChangCounterDto != null) nameChangCounterDtos.Remove(nameChangCounterDto);
            var seasonNameChangCounterDtos = nameChangCounterDtos.Append(seasonNameChangCounterDto);
            var orderedList = seasonNameChangCounterDtos.OrderByDescending(dto => dto.NameChangesAmount);
            var top10NameChangers = orderedList.Take(10);
            return top10NameChangers.ToList();
        }

        public async Task HandleAsync(SeasonNameChangedEvent domainEvent)
        {
            var result = await _qeryRepository.Load<Top10NameChangers>();
            if (result.Is<NotFound<Top10NameChangers>>()) result = Result<Top10NameChangers>.Ok(new Top10NameChangers());

            var allEvents = (await _streamRepository.LoadEventsByEntity(domainEvent.EntityId)).Value;
            var ammountOfNameChanges = allEvents.Where(ev => ev.GetType() == typeof(SeasonNameChangedEvent)).Count();

            var top10NameChangers = result.Value;
            var seasonNameChangedEvents = SortIn(top10NameChangers.SeasonCounter, new SeasonNameChangCounterDto(domainEvent.EntityId, ammountOfNameChanges));
            top10NameChangers.SeasonCounter = seasonNameChangedEvents;

            await _qeryRepository.Save(top10NameChangers);
        }
    }
}