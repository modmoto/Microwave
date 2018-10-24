using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Application.Framework;
using Microsoft.EntityFrameworkCore;

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
            var query = await _context.Querries.FirstOrDefaultAsync(queryDbo => queryDbo.Type == name);
            return _converter.Deserialize<T>(query.Payload);
        }

        public async Task<T> Load<T>(Guid id) where T : IdentifiableQuery
        {
            var querry = await _context.IdentifiableQuerries.FirstOrDefaultAsync(query => query.Id == id);
            return _converter.Deserialize<T>(querry.Payload);
        }

        public async Task Save(Query query)
        {
            var queryDbo = new QueryDbo
            {
                Type = query.Type,
                Payload = _converter.Serialize(query)
            };

            await _context.Querries.Upsert(queryDbo).RunAsync();
            await _context.SaveChangesAsync();
        }

        public async Task Save(IdentifiableQuery query)
        {
            var identifiableQueryDbo = new IdentifiableQueryDbo
            {
                Id = query.Id,
                Payload = _converter.Serialize(query)
            };

            _context.IdentifiableQuerries.Upsert(identifiableQueryDbo).Run();
            await _context.SaveChangesAsync();
        }
    }
}