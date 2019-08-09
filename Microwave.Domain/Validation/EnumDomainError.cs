using System;

namespace Microwave.Domain.Validation
{
    public class EnumDomainError : DomainError
    {
        private readonly Enum _enumErrorType;

        internal EnumDomainError(Enum enumErrorType, string detail = null)
        {
            Detail = detail;
            _enumErrorType = enumErrorType;
        }

        public override string ErrorType => _enumErrorType.ToString();
        public override string Detail { get; }
    }
}