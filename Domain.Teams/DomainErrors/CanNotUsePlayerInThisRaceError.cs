using System;
using Domain.Framework;

namespace Domain.Teams
{
    public class CanNotUsePlayerInThisRaceError : DomainError
    {
        public CanNotUsePlayerInThisRaceError(Guid playerTypeId, Guid raceId) : base($"Can not use playertyp: {playerTypeId} in Race {raceId}.")
        {
        }
    }
}