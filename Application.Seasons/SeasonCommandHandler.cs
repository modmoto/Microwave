using System;
using System.Threading.Tasks;
using Application.Framework;
using Application.Framework.Exceptions;
using Application.Seasons.Commands;
using Domain.Seasons;

namespace Application.Seasons
{
    public class SeasonCommandHandler : CommandHandler
    {
        public SeasonCommandHandler(IEventStore eventStore) : base(eventStore)
        {
        }

        public async Task CreateSeason(CreateSesonCommand command)
        {
            var domainResult = Season.Create(command.SeasonName, command.MaxDaysBetweenGames);
            if (domainResult.Failed) throw new DomainValidationException(domainResult.DomainErrors);
            await EventStore.AppendAsync(domainResult.DomainEvents);
        }

        public async Task SetStartAndEndDate(ChangeDateCommand command)
        {
            var season = await EventStore.LoadAsync<Season>(command.EntityId);
            var domainResult = season.ChangeDate(command.StartDate, command.EndDate);
            if (domainResult.Failed) throw new DomainValidationException(domainResult.DomainErrors);
            await EventStore.AppendAsync(domainResult.DomainEvents);
        }
    }

    public class ChangeDateCommand
    {
        public Guid EntityId { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
    }
}