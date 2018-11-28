using Microwave.Application.Exceptions;

namespace Microwave.Application.Results
{
    public class NotFound<T> : Result<T>
    {
        public NotFound(string notFoundId)
        {
            NotFoundId = notFoundId;
        }

        public override T Value => throw new NotFoundException(typeof(T), NotFoundId);

        public string NotFoundId { get; }
    }
}