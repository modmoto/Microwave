using System;

namespace Microwave.Domain.Validation
{
    public abstract class DomainError
    {
        protected DomainError(string detail = null)
        {
            ErrorType = GetType().Name;
            Detail = detail;
        }

        internal DomainError(string errorType = null, string detail = null)
        {
            ErrorType = errorType ?? GetType().Name;
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

        public string ErrorType { get; }
        public string Detail { get; }
    }
}