using System;

namespace Microwave.Domain.Validation
{
    public class EnumDomainError : DomainError
    {
        private readonly Enum _enumErrorType;

        public EnumDomainError(Enum enumErrorType)
        {
            _enumErrorType = enumErrorType;
        }

        public override string ErrorType => _enumErrorType.ToString();
    }
}