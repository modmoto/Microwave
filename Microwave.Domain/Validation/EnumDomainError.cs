using System;

namespace Microwave.Domain.Validation
{
    public class EnumDomainError : DomainError
    {
        internal EnumDomainError(Enum enumErrorType, string detail = null) : base(detail, enumErrorType.ToString())
        {
        }
    }
}