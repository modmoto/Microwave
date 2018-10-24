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
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<QueryDbo>()
                .Property(b => b.RowVersion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAddOrUpdate();
        }
    }

    public class IdentifiableQueryDbo
    {
        public Guid Id { get; set; }
        public string Payload { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }

    public class QueryDbo
    {
        [Key]
        public string Type { get; set; }
        public string Payload { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}