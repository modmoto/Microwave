using System;

namespace Microwave.Domain.Validation
{
    public abstract class DomainError
    {
        public DomainError(string description = null)
        {
            Description = description;
        }

        public virtual string ErrorType => GetType().Name;
        public string Description { get; }
    }

    internal class TypelessDomainError : DomainError
    {
        public TypelessDomainError(string errorType)
        {
            ErrorType = errorType;
        }

        public override string ErrorType { get; }
    }

    internal class EnumDomainError : DomainError
    {
        private readonly Enum _enumErrorType;

        public EnumDomainError(Enum enumErrorType)
        {
            _enumErrorType = enumErrorType;
        }

        public override string ErrorType => _enumErrorType.ToString();
    }
}