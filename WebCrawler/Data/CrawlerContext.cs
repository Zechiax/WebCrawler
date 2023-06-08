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
        var websiteGraphConverter = new ValueConverter<WebsiteGraph, string>(
            v => WebsiteGraphSnapshot.JsonConverter.Serialize(v.GetSnapshot()),
            v => new WebsiteGraph(WebsiteGraphSnapshot.JsonConverter.Deserialize(v).EntryWebsite!) 
        );

        modelBuilder.Entity<WebsiteRecord>()
            .HasOne(e => e.CrawlInfo)
            .WithOne()
            .HasForeignKey<CrawlInfo>(c => c.WebsiteRecordId) 
            .IsRequired();

        modelBuilder.Entity<WebsiteRecord>()
            .HasMany(e => e.Tags)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "TagWebsiteRecord",
                r => r.HasOne<Tag>().WithMany().HasForeignKey("TagsId").OnDelete(DeleteBehavior.Cascade),
                l => l.HasOne<WebsiteRecord>().WithMany().HasForeignKey("WebsiteRecordsId").OnDelete(DeleteBehavior.Cascade)
            );

        modelBuilder.Entity<CrawlInfo>()
            .Property(e => e.RegexPattern)
            .HasDefaultValue(".*");
    
        modelBuilder.Ignore<WebsiteExecution>(); // Add this line

        modelBuilder.Entity<CrawlInfo>()
            .HasOne(e => e.LastExecution)
            .WithOne()
            .HasForeignKey<WebsiteExecution>("CrawlInfoId");

        modelBuilder.Entity<WebsiteExecution>()
            .Property(e => e.WebsiteGraph)
            .HasConversion(websiteGraphConverter!);
    }


}