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
    public DbSet<CrawlInfoData> CrawlInfos { get; set; } = null!;
    
    public CrawlerContext(DbContextOptions<CrawlerContext> options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WebsiteRecordData>()
            .HasOne(e => e.CrawlInfoData)
            .WithOne()
            .HasForeignKey<CrawlInfoData>(c => c.WebsiteRecordDataId) 
            .IsRequired();

        modelBuilder.Entity<WebsiteRecordData>()
            .HasMany(e => e.Tags)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "TagWebsiteRecord",
                r => r.HasOne<Tag>().WithMany().HasForeignKey("TagsId").OnDelete(DeleteBehavior.Cascade),
                l => l.HasOne<WebsiteRecordData>().WithMany().HasForeignKey("WebsiteRecordsDataId").OnDelete(DeleteBehavior.Cascade)
            );

        modelBuilder.Entity<CrawlInfoData>()
            .Property(e => e.RegexPattern)
            .HasDefaultValue(".*");
    
        modelBuilder.Ignore<WebsiteExecutionData>(); // Add this line

        modelBuilder.Entity<CrawlInfoData>()
            .HasOne(e => e.LastExecutionData)
            .WithOne()
            .HasForeignKey<WebsiteExecutionData>("CrawlInfoDataId");
    }


}