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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<QueryDbo>()
                .Property(p => p.Version)
                .IsConcurrencyToken();

            modelBuilder.Entity<IdentifiableQueryDbo>()
                .Property(p => p.Version)
                .IsConcurrencyToken();
        }
    }

    public class IdentifiableQueryDbo
    {
        public Guid Id { get; set; }
        public string Payload { get; set; }
        public long Version { get; set; }
    }

    public class QueryDbo
    {
        [Key]
        public string Type { get; set; }
        public string Payload { get; set; }
        public long Version { get; set; }
    }
}