using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Adapters.Framework.EventStores
{
    public sealed class QueryContext : DbContext
    {
        public DbSet<QueryDbo> Querries { get; set; }
        public DbSet<IdentifiableQueryDbo> IdentifiableQuerries { get; set; }

        public QueryContext(DbContextOptions<EventStoreContext> options) :
            base(options)
        {
        }
    }

    public class IdentifiableQueryDbo
    {
        public Guid Id { get; set; }
        public string Payload { get; set; }
    }

    public class QueryDbo
    {
        [Key]
        public string Id { get; set; }
        public string Payload { get; set; }
    }
}