using System;

namespace Microwave.Domain.Validation
{
    public class EnumDomainErrorRenamed : DomainErrorRenamed
    {
        private readonly Enum _enumErrorType;

        public EnumDomainErrorRenamed(Enum enumErrorType)
        {
            _enumErrorType = enumErrorType;
        }

        public override string ErrorType => _enumErrorType.ToString();
    }
}