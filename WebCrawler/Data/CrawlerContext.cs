using Microsoft.EntityFrameworkCore;
using WebCrawler.Models;

namespace WebCrawler.Data;

public class CrawlerContext : DbContext
{
    public DbSet<WebsiteRecord> WebsiteRecords { get; set; } = null!;
    public DbSet<Tag> Tags { get; set; } = null!;
    public DbSet<WebsiteExecution> Executions { get; set; } = null!;
    
    public CrawlerContext(DbContextOptions<CrawlerContext> options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // We define the one-to-one relationship between WebsiteRecord and WebsiteExecution.
        modelBuilder.Entity<WebsiteRecord>()
            .HasOne(e => e.LastExecution)
            .WithOne()
            .HasForeignKey<WebsiteExecution>("WebsiteRecordId")
            .IsRequired();
        
        // We define the tags and website records as a many-to-many relationship.
        modelBuilder.Entity<WebsiteRecord>()
            .HasMany(e => e.Tags)
            .WithMany(e => e.WebsiteRecords);
        
        modelBuilder.Entity<WebsiteExecution>()
            .HasOne(e => e.Info)
            .WithOne()
            .HasForeignKey<WebsiteExecution>("CrawlInfoId")
            .IsRequired();
        
        // We set the default value for the Regex of the CrawlInfo to '.*', so that it matches everything.
        modelBuilder.Entity<CrawlInfo>()
            .Property(e => e.RegexPattern)
            .HasDefaultValue(".*");
    }
}