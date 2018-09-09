using System.Threading.Tasks;
using Application.Framework;
using Application.Framework.Exceptions;
using Application.Seasons.Commands;
using Domain.Seasons;

namespace Application.Seasons
{
    public class SeasonCommandHandler : CommandHandler
    {
        public SeasonCommandHandler(IEventStoreFacade eventStoreFacade) : base(eventStoreFacade)
        {
        }

        public async Task CreateSeason(CreateSesonCommand command)
        {
            var domainResult = Season.Create(command.SeasonName, command.MaxDaysBetweenGames);
            if (domainResult.Failed) throw new DomainValidationException(domainResult.DomainErrors);
            await EventStoreFacade.AppendAsync(domainResult.DomainEvents);
        }

        public async Task SetStartAndEndDate(ChangeDateCommand command)
        {
            var seasonResult = await EventStoreFacade.LoadAsync<Season>(command.EntityId);
            var season = seasonResult.Result;
            var domainResult = season.ChangeDate(command.StartDate, command.EndDate);
            if (domainResult.Failed) throw new DomainValidationException(domainResult.DomainErrors);
            await EventStoreFacade.AppendAsync(domainResult.DomainEvents, seasonResult.EntityVersion);
        }

        public async Task ChangeName(ChangeNameCommand command)
        {
            var seasonResult = await EventStoreFacade.LoadAsync<Season>(command.EntityId);
            var season = seasonResult.Result;
            var domainResult = season.ChangeName(command.Name);
            if (domainResult.Failed) throw new DomainValidationException(domainResult.DomainErrors);
            await EventStoreFacade.AppendAsync(domainResult.DomainEvents, seasonResult.EntityVersion);
        }
    }
}