using System.Reflection;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using WebCrawler.Data;
using WebCrawler.Interfaces;
using WebCrawler.Models;
using Serilog;
using WebCrawler.Formatters;
using WebCrawler.Services;

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

// We add Serilog to the builder.
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.Debug()
    .WriteTo.Async(a => a.File("logs/log.txt", rollingInterval: RollingInterval.Day))
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(Log.Logger);
builder.Host.UseSerilog();

builder.Services.AddDbContext<CrawlerContext>(options =>
{
    options.UseSqlite("Data Source=WebCrawler.db");
});
builder.Services.AddSingleton<IDataService, DataService>();

builder.Services.AddSingleton<IPeriodicExecutionManagerService, PeriodicExecutionManagerService>();

//builder.Services.AddSingleton<IExecutionManagerService, ExecutionManagerService>();

builder.Services.AddMvcCore(options =>
{
    options.InputFormatters.Insert(0, new JsonFromBodyFormatter());
});

builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSerilogRequestLogging();

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
    var data = scope.ServiceProvider.GetRequiredService<IDataService>();
    await data.MigrateAsync();
}

// Let's seed some initial data for development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var data = scope.ServiceProvider.GetRequiredService<IDataService>();
    var context = scope.ServiceProvider.GetRequiredService<CrawlerContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Seeding data");
    await SeedData.SeedDataAsync(context, data);
    logger.LogInformation("Data seeded");
}

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

