using System;
using Microwave.Domain.Exceptions;

namespace Microwave.Domain.Results
{
    public class NotFoundResult<T> : Result<T>
    {
        public NotFoundResult(Identity notFoundId) : base(new NotFound(typeof(T), notFoundId))
        {
        }
    }

    public class NotFound : ResultStatus
    {
        public Type Type { get; }
        public Identity NotFoundId { get; }

        public NotFound(Type type, Identity notFoundId)
        {
            Type = type;
            NotFoundId = notFoundId;
        }

        public override void Check()
        {
            throw new NotFoundException(Type, NotFoundId.Id);
        }
    }
}