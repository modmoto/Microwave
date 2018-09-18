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
            var domainResult = Season.Create(command.SeasonName);
            if (domainResult.Failed) throw new DomainValidationException(domainResult.DomainErrors);
            await EventStoreFacade.AppendAsync(domainResult.DomainEvents, 0);
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