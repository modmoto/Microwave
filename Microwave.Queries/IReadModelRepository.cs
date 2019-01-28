using System.Threading.Tasks;
using Microwave.Application.Results;
using Microwave.Domain;

namespace Microwave.Queries
{
    public interface IReadModelRepository
    {
        Task<Result<T>> Load<T>() where T : Query;
        Task<ReadModelWrapper<T>> Load<T>(Identity id) where T : ReadModel;
        Task<Result> Save<T>(T query) where T : Query;
        Task<Result> Save<T>(ReadModelWrapper<T> readModelWrapper) where T : ReadModel, new();
    }

    public class ReadModelWrapper<T>
    {
        protected ReadModelWrapper(ResultStatus status, T readModel, long version, Identity id)
        {
            _version = version;
            _readModel = readModel;
            Status = status;
            _id = id;
        }

        public ReadModelWrapper(T readModel, Identity id, long version)
        {
            _version = version;
            _readModel = readModel;
            Status = new Ok();
            _id = id;
        }

        public long Version
        {
            get
            {
                Status.Check();
                return _version;
            }
        }

        private T _readModel;
        private readonly long _version;
        private readonly Identity _id;

        protected ResultStatus Status { get; }

        public Identity Id
        {
            get
            {
                Status.Check();
                return _id;
            }
        }

        public T ReadModel
        {
            get
            {
                Status.Check();
                return _readModel;
            }
        }

        public static ReadModelWrapper<T> Ok(T value, Identity id, long version)
        {
            return new Ok<T>(value, version, id);
        }

        public bool Is<TCheck>() where TCheck : ResultStatus
        {
            return typeof(TCheck) == Status.GetType();
        }

        public static ReadModelWrapper<T> NotFound(Identity notFoundId)
        {
            return new NotFoundResult<T>(notFoundId);
        }
    }

    public class Ok<T> : ReadModelWrapper<T>
    {
        public Ok(T readModel, long version, Identity id) : base(new Ok(), readModel, version, id)
        {
        }
    }

    public class NotFoundResult<T> : ReadModelWrapper<T>
    {
        public NotFoundResult(Identity notFoundId) : base(new NotFound(typeof(T), notFoundId), default(T), -1, null)
        {
        }
    }
}