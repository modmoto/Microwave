using Microwave.Domain;

namespace TestHost.Write.Domain.DomainErrors
{
    public class CanNotUsePlayerInThisRaceError : DomainError
    {
        public CanNotUsePlayerInThisRaceError(Identity playerTypeId) : base($"Can not use playertyp: {playerTypeId.Id} in this team.")
        {
        }
    }
}