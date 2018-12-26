using Microwave.Domain;

namespace TestHost.Read.DomainEvents
{
    public class PlayerBought : IDomainEvent
    {
        public Identity EntityId{ get; set; }
        public GoldCoins NewTeamChestBalance{ get; set; }
        public Identity PlayerTypeId{ get; set; }

        public PlayerBought(Identity entityId, Identity playerTypeId, GoldCoins newTeamChestBalance)
        {
            EntityId = entityId;
            NewTeamChestBalance = newTeamChestBalance;
            PlayerTypeId = playerTypeId;
        }
    }
}