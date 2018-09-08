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
            var season = await EventStoreFacade.LoadAsync<Season>(command.EntityId);
            var domainResult = season.ChangeDate(command.StartDate, command.EndDate);
            if (domainResult.Failed) throw new DomainValidationException(domainResult.DomainErrors);
            await EventStoreFacade.AppendAsync(domainResult.DomainEvents);
        }

        public async Task ChangeName(ChangeNameCommand command)
        {
            var season = await EventStoreFacade.LoadAsync<Season>(command.EntityId);
            var domainResult = season.ChangeName(command.Name);
            if (domainResult.Failed) throw new DomainValidationException(domainResult.DomainErrors);
            await EventStoreFacade.AppendAsync(domainResult.DomainEvents);
        }
    }
}