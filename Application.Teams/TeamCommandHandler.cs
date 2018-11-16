using System;
using System.Threading.Tasks;
using Application.Framework;
using Domain.Teams;

namespace Application.Teams
{
    public class TeamCommandHandler
    {
        private readonly IEventStore _eventStore;

        public TeamCommandHandler(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public async Task CreateTeam(CreateTeamComand createTeamComand)
        {
            var eventStoreResult = await _eventStore.LoadAsync<Race>(createTeamComand.RaceId);
            var race = eventStoreResult.Value;
            var domainResult = Team.Create(race.Id, createTeamComand.TeamName, createTeamComand.PlayerName);
            await _eventStore.AppendAsync(domainResult.DomainEvents, -1);
        }

        public async Task BuyPlayer(BuyPlayerComand buyPlayerComand)
        {
            var teamResult = await _eventStore.LoadAsync<Team>(buyPlayerComand.TeamId);
            var team = teamResult.Value;
            var buyPlayer = team.BuyPlayer(buyPlayerComand.PlayerTypeId);
            await _eventStore.AppendAsync(buyPlayer.DomainEvents, teamResult.Version);
        }
    }

    public class BuyPlayerComand
    {
        public Guid TeamId { get; set; }
        public Guid PlayerTypeId { get; set; }
    }

    public class CreateTeamComand
    {
        public string PlayerName { get; set; }
        public string TeamName { get; set; }
        public Guid RaceId { get; set; }
    }
}