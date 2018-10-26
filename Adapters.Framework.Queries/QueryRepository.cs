using System;
using System.Threading.Tasks;
using Application.Framework;
using Application.Framework.Results;

namespace Adapters.Framework.Queries
{
    public class QueryRepository : IQeryRepository
    {
        private readonly QueryStorageContext _context;
        private readonly IObjectConverter _converter;

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
            data.Version = query.Version;
            return Result<T>.Ok(data);
        }

        public async Task<Result<T>> Load<T>(Guid id) where T : IdentifiableQuery
        {
            var querry = await _context.IdentifiableQuerries.FindAsync(id);
            if (querry == null) return Result<T>.NotFound(id.ToString());
            var deserialize = _converter.Deserialize<T>(querry.Payload);
            deserialize.Version = querry.Version;
            return Result<T>.Ok(deserialize);
        }

        public async Task<Result> Save(Query query)
        {
            var firstOrDefault = await _context.Querries.FindAsync(query.Type);
            if (firstOrDefault != null)
            {
                if (firstOrDefault.Version != query.Version)
                    return Result.ConcurrencyResult(firstOrDefault.Version, query.Version);
                firstOrDefault.Payload = _converter.Serialize(query);
                firstOrDefault.Version++;
                _context.Update(firstOrDefault);
            }
            else
            {
                var queryDbo = new QueryDbo
                {
                    Type = query.Type,
                    Version = 0L,
                    Payload = _converter.Serialize(query)
                };
                await _context.Querries.AddAsync(queryDbo);
            }

            await _context.SaveChangesAsync();
            return Result.Ok();
        }

        public async Task<Result> Save(IdentifiableQuery query)
        {
            var firstOrDefault = await _context.IdentifiableQuerries.FindAsync(query.Id);
            if (firstOrDefault != null)
            {
                if (firstOrDefault.Version != query.Version)
                    return Result.ConcurrencyResult(firstOrDefault.Version, query.Version);
                firstOrDefault.Payload = _converter.Serialize(query);
                firstOrDefault.Version++;
                _context.Update(firstOrDefault);
            }
            else
            {
                var identifiableQueryDbo = new IdentifiableQueryDbo
                {
                    Id = query.Id,
                    Version = 0L,
                    Payload = _converter.Serialize(query)
                };
                await _context.IdentifiableQuerries.AddAsync(identifiableQueryDbo);
            }

            await _context.SaveChangesAsync();
            return Result.Ok();
        }
    }
}