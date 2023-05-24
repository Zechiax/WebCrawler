using System.Reflection;
using Microsoft.EntityFrameworkCore;
using WebCrawler.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

builder.Services.AddDbContext<CrawlerContext>(options =>
{
    options.UseSqlite("Data Source=WebCrawler.db");
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");

// For when we create database migrations
if (args.Length > 0 && args[0].Equals("migration", StringComparison.InvariantCultureIgnoreCase))
{
    // We don't need the app to run
    return;
}

// We apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Applying migrations...");
    var context = scope.ServiceProvider.GetRequiredService<CrawlerContext>();
    context.Database.Migrate();
    logger.LogInformation("Migrations applied");
}

app.Run();