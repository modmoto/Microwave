using System.Collections.Generic;
using Microwave.Domain;
using Microwave.TestHostWrite.Domain.DomainErrors;

namespace Microwave.TestHostWrite.Domain
{
    public class AllowedPlayer
    {
        public AllowedPlayer(StringIdentity playerTypeId, int maximumPlayers, GoldCoins cost, string playerDescription)
        {
            PlayerTypeId = playerTypeId;
            MaximumPlayers = maximumPlayers;
            Cost = cost;
            PlayerDescription = playerDescription;
        }

        public DomainResult CanUsePlayer(int ammount)
        {
            var moreThanMax = ammount < MaximumPlayers;

            if (!moreThanMax) return DomainResult.Error(new TeamFullError(MaximumPlayers));

            return DomainResult.Ok(new List<IDomainEvent>());
        }

        public StringIdentity PlayerTypeId { get; }
        public int MaximumPlayers { get; }
        public GoldCoins Cost { get; }
        public string PlayerDescription { get; }
    }
}