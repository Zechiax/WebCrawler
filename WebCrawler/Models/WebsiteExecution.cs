﻿using System.ComponentModel.DataAnnotations;

namespace WebCrawler.Models;

public class WebsiteExecution
{
    public DateTime? Started { get; set; }
    public DateTime? Finished { get; set; }
    public WebsiteGraph? WebsiteGraph { get; set; }

    /// <summary>
    /// Constructor for EF Core
    /// </summary>
    public WebsiteExecution() { }
}
