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
            if (query == null) return null;
            var data = _converter.Deserialize<T>(query.Payload);
            data.Version = query.RowVersion;
            return data;
        }

        public async Task<T> Load<T>(Guid id) where T : IdentifiableQuery
        {
            var querry = await _context.IdentifiableQuerries.FirstOrDefaultAsync(query => query.Id == id);
            var deserialize = _converter.Deserialize<T>(querry.Payload);
            deserialize.Version = querry.RowVersion;
            return deserialize;
        }

        public async Task Save(Query query)
        {
            var queryDbo = new QueryDbo
            {
                Type = query.Type,
                RowVersion = query.Version,
                Payload = _converter.Serialize(query)
            };

            var firstOrDefault = _context.Querries.FirstOrDefault(q => q.Type == query.Type);
            if (firstOrDefault != null)
            {
                firstOrDefault.Payload = _converter.Serialize(query);
            }
            else await _context.Querries.AddAsync(queryDbo);

            await _context.SaveChangesAsync();
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