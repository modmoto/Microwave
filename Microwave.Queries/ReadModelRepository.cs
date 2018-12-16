using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microwave.Application;
using Microwave.Application.Results;

namespace Microwave.Queries
{
    public class ReadModelRepository : IReadModelRepository
    {
        private readonly ReadModelStorageContext _context;
        private readonly IObjectConverter _converter;

        public ReadModelRepository(ReadModelStorageContext context, IObjectConverter converter)
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

        public Task<Result<IEnumerable<ReadModelWrapper<T>>>> LoadAll<T>() where T : ReadModel
        {
            var querries = _context.IdentifiableQuerries.Where(q => q.QueryType == typeof(T).Name);
            var readModelWrappers = querries.Select(q =>
                new ReadModelWrapper<T>(_converter.Deserialize<T>(q.Payload), new Guid(q.Id), q.Version));
            return Task.FromResult(Result<IEnumerable<ReadModelWrapper<T>>>.Ok(readModelWrappers));
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

        public async Task<Result> Save<T>(ReadModelWrapper<T> readModelWrapper) where T : ReadModel, new()
        {
            await _context.IdentifiableQuerries.Upsert(new IdentifiableQueryDbo
                {
                    Id = readModelWrapper.Id.ToString(),
                    Version = readModelWrapper.Version,
                    QueryType = readModelWrapper.ReadModel.GetType().Name,
                    Payload = _converter.Serialize(readModelWrapper.ReadModel)
                })
                .RunAsync();
            return Result.Ok();
        }
    }
}