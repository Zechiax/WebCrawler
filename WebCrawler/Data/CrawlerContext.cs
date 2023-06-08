using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WebCrawler.Models;

namespace WebCrawler.Data;

public class CrawlerContext : DbContext
{
    public DbSet<WebsiteRecord> WebsiteRecords { get; set; } = null!;
    public DbSet<Tag> Tags { get; set; } = null!;
    public DbSet<WebsiteExecution> Executions { get; set; } = null!;
    public DbSet<CrawlInfo> CrawlInfos { get; set; } = null!;
    
    public CrawlerContext(DbContextOptions<CrawlerContext> options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // We add custom value converter for WebsiteExecution
        // We currently store the WebsiteGraph as a JSON string in the database.
        var websiteExecutionConverter = new ValueConverter<WebsiteGraph, string>(
            v => WebsiteGraphSnapshot.JsonConverter.Serialize(v.GetSnapshot()),
            v => new WebsiteGraph(WebsiteGraphSnapshot.JsonConverter.Deserialize(v).EntryWebsite!) 
        );

        // We define the one-to-one relationship between WebsiteRecord and WebsiteExecution.
        modelBuilder.Entity<WebsiteRecord>()
            .HasOne(e => e.CrawlInfo)
            .WithOne()
            .HasForeignKey<WebsiteRecord>("CrawlInfoId")
            .IsRequired();
        
        // We define the tags and website records as a many-to-many relationship.
        modelBuilder.Entity<WebsiteRecord>()
            .HasMany(e => e.Tags)
            .WithMany(e => e.WebsiteRecords);

        modelBuilder.Entity<WebsiteExecution>()
            .Property(e => e.WebsiteGraph)
            .HasConversion(websiteExecutionConverter!);

        // We set the default value for the Regex of the CrawlInfo to '.*', so that it matches everything.
        modelBuilder.Entity<CrawlInfo>()
            .Property(e => e.RegexPattern)
            .HasDefaultValue(".*");
        
        modelBuilder.Entity<CrawlInfo>()
            .HasOne(e => e.LastExecution)
            .WithOne()
            .HasForeignKey<CrawlInfo>("LastExecutionId")
            .IsRequired(false);
        
        modelBuilder.Entity<CrawlInfo>()
            .HasOne<WebsiteRecord>()
            .WithMany()
            .HasForeignKey(c => c.WebsiteRecordId);
    }
}