using System;

namespace Microwave.Domain.Validation
{
    public abstract class DomainError
    {
        protected DomainError(string detail = null)
        {
            Detail = detail;
        }

        public static DomainError Create(Enum errorType, string detail = null)
        {
            return new EnumDomainError(errorType, detail);
        }

        public static DomainError Create(string errorType, string detail = null)
        {
            return new TypelessDomainError(errorType, detail);
        }

        public virtual string ErrorType => GetType().Name;
        public virtual string Detail { get; }
    }
}