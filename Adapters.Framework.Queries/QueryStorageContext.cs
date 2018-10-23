using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Adapters.Framework.Queries
{
    public sealed class QueryStorageContext : DbContext
    {
        public DbSet<QueryDbo> Querries { get; set; }
        public DbSet<IdentifiableQueryDbo> IdentifiableQuerries { get; set; }

        public QueryStorageContext(DbContextOptions<QueryStorageContext> options) :
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
        public string Type { get; set; }
        public string Payload { get; set; }
    }
}