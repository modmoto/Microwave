using System;
using System.Linq;
using System.Threading.Tasks;
using Application.Framework;
using Microsoft.EntityFrameworkCore;

namespace Adapters.Framework.Queries
{
    public class QeryRepository : IQeryRepository
    {
        private readonly QueryStorageContext _context;
        private readonly IObjectConverter _converter;

        public QeryRepository(QueryStorageContext context, IObjectConverter converter)
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

        public Task Save(Query query)
        {
//            var querry = await _context.IdentifiableQuerries.FirstOrDefaultAsync(query => query.Id == query.GetType().Name);
//            if (querry == null)
//            {
//
//            }

            return null;
        }

        public Task Save(IdentifiableQuery query)
        {
            return null;
        }
    }
}