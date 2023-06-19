using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WebCrawler.Models;
using WebCrawler.Models.Database;

namespace WebCrawler.Data;

public class CrawlerContext : DbContext
{
    public DbSet<WebsiteRecordData> WebsiteRecords { get; set; } = null!;
    public DbSet<Tag> Tags { get; set; } = null!;
    public DbSet<WebsiteExecutionData> Executions { get; set; } = null!;
    public DbSet<CrawlInfo> CrawlInfos { get; set; } = null!;
    
    public CrawlerContext(DbContextOptions<CrawlerContext> options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WebsiteRecordData>()
            .HasOne(e => e.CrawlInfo)
            .WithOne()
            .HasForeignKey<CrawlInfo>(c => c.WebsiteRecordId) 
            .IsRequired();

        modelBuilder.Entity<WebsiteRecordData>()
            .HasMany(e => e.Tags)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "TagWebsiteRecord",
                r => r.HasOne<Tag>().WithMany().HasForeignKey("TagsId").OnDelete(DeleteBehavior.Cascade),
                l => l.HasOne<WebsiteRecordData>().WithMany().HasForeignKey("WebsiteRecordsId").OnDelete(DeleteBehavior.Cascade)
            );

        modelBuilder.Entity<CrawlInfo>()
            .Property(e => e.RegexPattern)
            .HasDefaultValue(".*");
    
        modelBuilder.Ignore<WebsiteExecutionData>(); // Add this line

        modelBuilder.Entity<CrawlInfo>()
            .HasOne(e => e.LastExecution)
            .WithOne()
            .HasForeignKey<WebsiteExecutionData>("CrawlInfoId");
    }


}