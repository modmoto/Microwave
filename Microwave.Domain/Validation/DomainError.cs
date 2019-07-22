using System;

namespace Microwave.Domain.Validation
{
    public abstract class DomainError
    {
        protected DomainError(string description = null)
        {
            Description = description;
        }

        public static DomainError Create(Enum errorType)
        {
            return new EnumDomainError(errorType);
        }

        public static DomainError Create(string errorType)
        {
            return new TypelessDomainError(errorType);
        }

        public virtual string ErrorType => GetType().Name;
        public string Description { get; }
    }
}