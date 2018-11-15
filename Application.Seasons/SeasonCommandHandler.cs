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
        private readonly IEventStore _eventStore;

        public SeasonCommandHandler(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public async Task<Guid> CreateSeason(CreateSesonCommand command)
        {
            var domainResult = Season.Create(command.SeasonName);
            await _eventStore.AppendAsync(domainResult.DomainEvents, -1);
            return domainResult.DomainEvents.First().EntityId;
        }

        public async Task ChangeName(ChangeNameCommand command)
        {
            var seasonResult = await _eventStore.LoadAsync<Season>(command.EntityId);
            var season = seasonResult;
            var domainResult = season.Value.ChangeName(command.Name);
            await _eventStore.AppendAsync(domainResult.DomainEvents, seasonResult.Version);
        }
    }
}