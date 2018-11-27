using Domain.Framework;

namespace Domain.Teams
{
    public class FewMoneyInTeamChestError : DomainError
    {
        public FewMoneyInTeamChestError(long playerCost, long teamMoney) : base($"Can not buy Player. Player costs {playerCost}, your chest only contains {teamMoney}")
        {
        }
    }
}