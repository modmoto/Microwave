using System.Collections.Generic;
using System.Threading.Tasks;
using Microwave.Application.Results;
using Microwave.Domain;

namespace Microwave.Queries
{
    public interface IReadModelRepository
    {
        Task<Result<T>> Load<T>() where T : Query;
        Task<ReadModelResult<T>> Load<T>(Identity id) where T : ReadModel;
        Task<Result> Save<T>(T query) where T : Query;
        Task<Result> Save<T>(ReadModelResult<T> readModelResult) where T : ReadModel, new();
        Task<Result<IEnumerable<T>>> LoadAll<T>() where T : ReadModel;
    }

    public class ReadModelResult<T> : Result<T>
    {
        protected ReadModelResult(ResultStatus status, T readModel, Identity id) : base(status)
        {
            _value = readModel;
            _id = id;
        }

        public ReadModelResult(T readModel, Identity id) : base(new Ok())
        {
            _value = readModel;
            _id = id;
        }

        private readonly Identity _id;

        public Identity Id
        {
            get
            {
                Status.Check();
                return _id;
            }
        }

        public static ReadModelResult<T> Ok(T value, Identity id)
        {
            return new Ok<T>(value, id);
        }

        public new static ReadModelResult<T> NotFound(Identity notFoundId)
        {
            return new NotFoundResult<T>(notFoundId);
        }
    }

    public class Ok<T> : ReadModelResult<T>
    {
        public Ok(T readModel, Identity id) : base(new Ok(), readModel, id)
        {
        }
    }

    public class NotFoundResult<T> : ReadModelResult<T>
    {
        public NotFoundResult(Identity notFoundId) : base(new NotFound(typeof(T), notFoundId), default(T), null)
        {
        }
    }
}