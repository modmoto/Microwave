using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Microwave.Queries
{
    public sealed class QueryStorageContext : DbContext
    {
        public DbSet<QueryDbo> Querries { get; set; }
        public DbSet<IdentifiableQueryDbo> IdentifiableQuerries { get; set; }
        public DbSet<LastProcessedVersionDbo> ProcessedVersions { get; set; }

        public QueryStorageContext(DbContextOptions<QueryStorageContext> options) :
            base(options)
        {
        }
    }

    public class IdentifiableQueryDbo
    {
        public string Id { get; set; }
        public string Payload { get; set; }
        public long Version { get; set; }
    }

    public class QueryDbo
    {
        [Key]
        public string Type { get; set; }
        public string Payload { get; set; }
    }

    public class LastProcessedVersionDbo
    {
        public LastProcessedVersionDbo(string eventType, long lastVersion)
        {
            EventType = eventType;
            LastVersion = lastVersion;
        }

        [Key] public string EventType { get; set; }

        public long LastVersion { get; set; }
    }
}