using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microwave.Application;
using Microwave.Application.Results;

namespace Microwave.Queries
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
            return Result<T>.Ok(data);
        }

        public async Task<Result<ReadModelWrapper<T>>> Load<T>(Guid id) where T : ReadModel
        {
            var querry = await _context.IdentifiableQuerries.FindAsync(id.ToString());
            if (querry == null || querry.QueryType != typeof(T).Name) return Result<ReadModelWrapper<T>>.NotFound(id.ToString());
            var deserialize = _converter.Deserialize<T>(querry.Payload);
            var wrapper = new ReadModelWrapper<T>(deserialize, id, querry.Version);
            return Result<ReadModelWrapper<T>>.Ok(wrapper);
        }

        public async Task<Result> Save(Query query)
        {
            await _context.Querries.Upsert(new QueryDbo
                {
                    Type = query.GetType().Name,
                    Payload = _converter.Serialize(query)
                })
                .RunAsync();
            return Result.Ok();
        }

        public async Task<Result> Save<T>(ReadModelWrapper<T> query) where T : ReadModel, new()
        {
            await _context.IdentifiableQuerries.Upsert(new IdentifiableQueryDbo
                {
                    Id = query.Id.ToString(),
                    Version = query.Version,
                    QueryType = query.ReadModel.GetType().Name,
                    Payload = _converter.Serialize(query.ReadModel)
                })
                .RunAsync();
            return Result.Ok();
        }
    }
}