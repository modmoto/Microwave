using System;
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
            var querry = await _context.Querries.FirstOrDefaultAsync(query => query.Type == typeof(T).Name);
            return _converter.Deserialize<T>(querry.Payload);
        }

        public async Task<T> Load<T>(Guid id) where T : IdentifiableQuery
        {
            var querry = await _context.IdentifiableQuerries.FirstOrDefaultAsync(query => query.Id == id);
            return _converter.Deserialize<T>(querry.Payload);
        }

        public async Task Save(Query query)
        {
            var queryDbo = await _context.Querries.FirstOrDefaultAsync(q => q.Type == query.GetType().Name);
            if (queryDbo == null)
            {
                _context.Querries.Add(new QueryDbo
                {
                    Payload = _converter.Serialize(query)
                });
            }
            else
            {
                queryDbo.Payload = _converter.Serialize(query);
                _context.Querries.Update(queryDbo);
            }
        }

        public async Task Save(IdentifiableQuery query)
        {
            var queryDbo = await _context.IdentifiableQuerries.FirstOrDefaultAsync(q => q.Id == query.Id);
            if (queryDbo == null)
            {
                _context.IdentifiableQuerries.Add(new IdentifiableQueryDbo
                {
                    Payload = _converter.Serialize(query)
                });
            }
            else
            {
                queryDbo.Payload = _converter.Serialize(query);
                _context.IdentifiableQuerries.Update(queryDbo);
            }
        }
    }
}