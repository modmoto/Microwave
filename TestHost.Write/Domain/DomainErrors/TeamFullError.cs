using Microwave.Domain;

namespace TestHost.Write.Domain.DomainErrors
{
    public class TeamFullError : DomainError
    {
        public TeamFullError(int maxmimumPlayers) : base($"Can not add more players than {maxmimumPlayers} for the PlayerType in this Race.")
        {
        }
    }
}