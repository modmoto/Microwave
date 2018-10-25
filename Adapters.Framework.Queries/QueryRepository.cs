using System;
using System.Linq;
using System.Threading.Tasks;
using Application.Framework;
using Application.Framework.Results;
using Microsoft.EntityFrameworkCore;

namespace Adapters.Framework.Queries
{
    public class QueryRepository : IQeryRepository
    {
        private readonly DbContextOptions<QueryStorageContext> _contextOptions;
        private readonly IObjectConverter _converter;

        public QueryRepository(DbContextOptions<QueryStorageContext> contextOptions, IObjectConverter converter)
        {
            _contextOptions = contextOptions;
            _converter = converter;
        }

        public async Task<T> Load<T>() where T : Query
        {
            using (var context = new QueryStorageContext(_contextOptions))
            {
                var name = typeof(T).Name;
                var query = await context.Querries.FindAsync(name);
                if (query == null) return null;
                var data = _converter.Deserialize<T>(query.Payload);
                data.Version = query.Version;
                return data;
            }
        }

        public async Task<T> Load<T>(Guid id) where T : IdentifiableQuery
        {
            using (var context = new QueryStorageContext(_contextOptions))
            {
                var querry = await context.IdentifiableQuerries.FindAsync(id);
                var deserialize = _converter.Deserialize<T>(querry.Payload);
                deserialize.Version = querry.Version;
                return deserialize;
            }
        }

        public async Task<Result> Save(Query query)
        {
            using (var context = new QueryStorageContext(_contextOptions))
            {
                var firstOrDefault = await context.Querries.FindAsync(query.Type);
                if (firstOrDefault != null)
                {
                    if (firstOrDefault.Version != query.Version)
                        return Result.ConcurrencyResult(firstOrDefault.Version, query.Version);
                    firstOrDefault.Payload = _converter.Serialize(query);
                    firstOrDefault.Version++;
                    context.Update(firstOrDefault);
                }
                else
                {
                    var queryDbo = new QueryDbo
                    {
                        Type = query.Type,
                        Version = 0L,
                        Payload = _converter.Serialize(query)
                    };
                    await context.Querries.AddAsync(queryDbo);
                }

                await context.SaveChangesAsync();
            }
            return Result.Ok();
        }

        public async Task Save(IdentifiableQuery query)
        {
            using (var context = new QueryStorageContext(_contextOptions))
            {
                var identifiableQueryDbo = new IdentifiableQueryDbo
                {
                    Id = query.Id,
                    Payload = _converter.Serialize(query)
                };

                var firstOrDefault = context.IdentifiableQuerries.FirstOrDefault(q => q.Id == query.Id);
                if (firstOrDefault != null) firstOrDefault.Payload = _converter.Serialize(query);
                else await context.IdentifiableQuerries.AddAsync(identifiableQueryDbo);

                await context.SaveChangesAsync();
            }
        }
    }
}