using Hangfire;
using HangfirePoc.Jobs;
using HangfirePoc.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseInMemoryStorage());

builder.Services.AddTransient<IRandomTextGenerator, RandomTextGenerator>();
builder.Services.AddHangfireServer(options => options.WorkerCount = Environment.ProcessorCount * 5);
builder.Services.AddSingleton<JobCoordinator>();

if (args.Contains("--console"))
    builder.Host.UseConsoleLifetime();

var app = builder.Build();

app.UseHangfireDashboard();

// Initialize recurring job
RecurringJob.AddOrUpdate(
    "database-monitor",
    () => app.Services.GetRequiredService<JobCoordinator>().MonitorDatabase(),
    "*/15 * * * * *" // Every 30 seconds
);

await app.RunAsync();