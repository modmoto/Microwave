using System;
using System.Linq;
using System.Threading.Tasks;
using Application.Framework;
using Application.Seasons.Commands;
using Domain.Seasons;

namespace Application.Seasons
{
    public class SeasonCommandHandler
    {
        private readonly IEventStoreFacade _eventStoreFacade;

        public SeasonCommandHandler(IEventStoreFacade eventStoreFacade)
        {
            _eventStoreFacade = eventStoreFacade;
        }

        public async Task<Guid> CreateSeason(CreateSesonCommand command)
        {
            var domainResult = Season.Create(command.SeasonName);
            await _eventStoreFacade.AppendAsync(domainResult.DomainEvents, -1);
            return domainResult.DomainEvents.First().EntityId;
        }

        public async Task ChangeName(ChangeNameCommand command)
        {
            var seasonResult = await _eventStoreFacade.LoadAsync<Season>(command.EntityId);
            var season = seasonResult;
            var domainResult = season.ChangeName(command.Name);
            await _eventStoreFacade.AppendAsync(domainResult.DomainEvents, season.Version);
        }
    }
}