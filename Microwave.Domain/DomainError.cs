namespace Microwave.Domain
{
    public class DomainError
    {
        public DomainError(string description = null)
        {
            Description = description;
            // test
        }

        public string ErrorType => GetType().Name;
        public string Description { get; }
    }
}