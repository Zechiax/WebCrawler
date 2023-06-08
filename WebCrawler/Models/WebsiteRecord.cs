﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebCrawler.Models;

/// <summary>
/// User provided data to describe crawling.
/// </summary>
[Table("WebsiteRecord")]
public class WebsiteRecord
{
    [Key]
    public int Id { get; set; }
    public TimeSpan Periodicity { get; set; } = TimeSpan.Zero;
    public string Label { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<Tag> Tags { get; set; } = new();
    public DateTime Created { get; set; } = DateTime.Now;
    public CrawlInfo CrawlInfo { get; set; } = null!;
}
