using System.Linq;
using System.Threading.Tasks;
using Microwave.Domain;
using Microwave.EventStores;
using Microwave.TestHostWrite.Domain;

namespace Microwave.TestHostWrite.Handler
{
    public class TeamCommandHandler
    {
        private readonly IEventStore _eventStore;

        public TeamCommandHandler(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public async Task<Identity> CreateTeam(CreateTeamCommand createTeamCommand)
        {
            var readModelResult = await _eventStore.LoadAsync<RaceConfig>(Identity.Create(createTeamCommand.RaceId));
            var race = readModelResult.Entity;
            var domainResult = Team.Create(race.Id, createTeamCommand.TeamName, createTeamCommand.TrainerName, race.AllowedPlayers);
            await _eventStore.AppendAsync(domainResult.DomainEvents, 0);
            return domainResult.DomainEvents.First().EntityId;
        }

        public async Task BuyPlayer(BuyPlayerCommand buyPlayerCommand)
        {
            var teamResult = await _eventStore.LoadAsync<Team>(buyPlayerCommand.TeamId);
            var team = teamResult.Entity;
            var buyPlayer = team.BuyPlayer(StringIdentity.Create(buyPlayerCommand.PlayerTypeId));
            await _eventStore.AppendAsync(buyPlayer.DomainEvents, buyPlayerCommand.TeamVersion);
        }
    }

    public class BuyPlayerCommand
    {
        public GuidIdentity TeamId { get; set; }
        public string PlayerTypeId { get; set; }
        public long TeamVersion { get; set; }
    }

    public class CreateTeamCommand
    {
        public string TrainerName { get; set; }
        public string TeamName { get; set; }
        public string RaceId { get; set; }
    }
}