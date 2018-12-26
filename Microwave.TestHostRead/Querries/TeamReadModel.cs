using System.Collections.Generic;
using System.Linq;
using Microwave.Domain;
using Microwave.Queries;
using Microwave.TestHostRead.DomainEvents;

namespace Microwave.TestHostRead.Querries
{
    [CreateReadmodelOn(typeof(TeamCreated))]
    public class TeamReadModel : ReadModel, IHandle<TeamCreated>, IHandle<PlayerBought>
    {
        public IEnumerable<Identity> PlayerList { get; set; }
        public Identity RaceId { get; set; }
        public string TrainerName { get; set; }
        public string TeamName { get; set; }
        public Identity TeamId { get; set; }

        public GoldCoins TeamChest { get; set; }

        public void Handle(TeamCreated domainEvent)
        {
            TeamId = domainEvent.EntityId;
            RaceId = domainEvent.RaceId;
            TeamName = domainEvent.TeamName;
            TrainerName = domainEvent.TrainerName;
            PlayerList = new List<Identity>();
        }

        public void Handle(PlayerBought domainEvent)
        {
            TeamChest = domainEvent.NewTeamChestBalance;
            PlayerList = PlayerList.Append(domainEvent.PlayerTypeId);
        }
    }
}