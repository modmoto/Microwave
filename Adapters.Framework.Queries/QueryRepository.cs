using System;
using System.Threading.Tasks;
using Microwave.Application;
using Microwave.Application.Results;

namespace Microwave.Queries
{
    public class QueryRepository : IQeryRepository
    {
        private readonly QueryStorageContext _context;
        private readonly IObjectConverter _converter;
        private readonly object _idQuerryLock = new object();
        private readonly object _querryLock = new object();

        public QueryRepository(QueryStorageContext context, IObjectConverter converter)
        {
            _context = context;
            _converter = converter;
        }

        public async Task<Result<T>> Load<T>() where T : Query
        {
            var name = typeof(T).Name;
            var query = await _context.Querries.FindAsync(name);
            if (query == null) return Result<T>.NotFound(name);
            var data = _converter.Deserialize<T>(query.Payload);
            return Result<T>.Ok(data);
        }

        public async Task<Result<T>> Load<T>(Guid id) where T : IdentifiableQuery
        {
            var querry = await _context.IdentifiableQuerries.FindAsync(id.ToString());
            if (querry == null) return Result<T>.NotFound(id.ToString());
            var deserialize = _converter.Deserialize<T>(querry.Payload);
            return Result<T>.Ok(deserialize);
        }

        //Todo Remove this locks again
        public async Task<Result> Save(Query query)
        {
            lock (_querryLock)
            {
                var firstOrDefault = _context.Querries.Find(query.GetType().Name);
                if (firstOrDefault != null)
                {
                    firstOrDefault.Payload = _converter.Serialize(query);
                    _context.Update(firstOrDefault);
                }
                else
                {
                    var queryDbo = new QueryDbo
                    {
                        Type = query.GetType().Name,
                        Payload = _converter.Serialize(query)
                    };
                    _context.Querries.Add(queryDbo);
                }

                _context.SaveChanges();
                return Result.Ok();
            }
        }

        public async Task<Result> Save(IdentifiableQuery query)
        {
            lock (_idQuerryLock)
            {
                var firstOrDefault = _context.IdentifiableQuerries.Find(query.Id.ToString());
                if (firstOrDefault != null)
                {
                    firstOrDefault.Payload = _converter.Serialize(query);
                    firstOrDefault.Id = query.Id.ToString();
                    _context.Update(firstOrDefault);
                }
                else
                {
                    var identifiableQueryDbo = new IdentifiableQueryDbo
                    {
                        Id = query.Id.ToString(),
                        Payload = _converter.Serialize(query)
                    };
                    _context.IdentifiableQuerries.Add(identifiableQueryDbo);
                }

                _context.SaveChanges();
                return Result.Ok();
            }
        }
    }
}