using System;
using System.Linq;
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

        public async Task<T> Load<T>() where T : Query
        {
            var name = typeof(T).Name;
            var query = await _context.Querries.FindAsync(name);
            if (query == null) return null;
            var data = _converter.Deserialize<T>(query.Payload);
            data.Version = query.Version;
            return data;
        }

        public async Task<T> Load<T>(Guid id) where T : IdentifiableQuery
        {
            var querry = await _context.IdentifiableQuerries.FindAsync(id);
            var deserialize = _converter.Deserialize<T>(querry.Payload);
            deserialize.Version = querry.Version;
            return deserialize;
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

        public async Task Save(IdentifiableQuery query)
        {
            var identifiableQueryDbo = new IdentifiableQueryDbo
            {
                Id = query.Id,
                Payload = _converter.Serialize(query)
            };

            var firstOrDefault = _context.IdentifiableQuerries.FirstOrDefault(q => q.Id == query.Id);
            if (firstOrDefault != null) firstOrDefault.Payload = _converter.Serialize(query);
            else await _context.IdentifiableQuerries.AddAsync(identifiableQueryDbo);

            await _context.SaveChangesAsync();
        }
    }
}