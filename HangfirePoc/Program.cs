using Hangfire;
using Hangfire.Server;
using Hangfire.States;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseInMemoryStorage());

builder.Services.AddHangfireServer();
builder.Services.AddSingleton<JobCoordinator>();

var app = builder.Build();

app.UseHangfireDashboard();

// Initialize recurring job
RecurringJob.AddOrUpdate(
    "database-monitor",
    () => app.Services.GetRequiredService<JobCoordinator>().MonitorDatabase(),
    "*/30 * * * * *" // Every 30 seconds
);

app.Run();

// JobCoordinator.cs
public sealed class JobCoordinator(
    IBackgroundJobClient backgroundJobClient,
    ILogger<JobCoordinator> logger)
{
    public Task MonitorDatabase()
    {
        Console.WriteLine("Monitoring Database");
        
        var options = new EnqueuedState { Queue = "handlers" };
        
        try
        {
            // Enqueue the three tasks
            backgroundJobClient.Enqueue<TaskExecutor>(
                job => job.ExecuteTask1(Guid.NewGuid()));

            backgroundJobClient.Enqueue<TaskExecutor>(
                job => job.ExecuteTask2(Guid.NewGuid()));
            
            backgroundJobClient.Enqueue<TaskExecutor>(
                job => job.ExecuteTask3(null, Guid.NewGuid()));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while creating jobs");
        }

        return Task.CompletedTask;
    }
}

// TaskExecutor.cs
public sealed class TaskExecutor(ILogger<TaskExecutor> logger)
{
    private readonly ILogger<TaskExecutor> _logger = logger;

    [AutomaticRetry(Attempts = 3, DelaysInSeconds = [5, 10])]
    public Task ExecuteTask1(Guid jobId)
    {
        Console.WriteLine($"Task1 - JobId: {jobId}");
        return Task.CompletedTask;
    }

    [AutomaticRetry(Attempts = 3, DelaysInSeconds = [5, 10])]
    public Task ExecuteTask2(Guid jobId)
    {
        Console.WriteLine($"Task2 - JobId: {jobId}");
        return Task.CompletedTask;
    }

    [AutomaticRetry(Attempts = 3, DelaysInSeconds = [5, 10])]
    public Task ExecuteTask3(PerformContext? context, Guid jobId)
    {
        int? retryCount = context?.GetJobParameter<int>("RetryCount");

        throw new InvalidOperationException($"Retry count is {retryCount}");

        Console.WriteLine($"ExecuteTask3 - JobId: {jobId} - Final Attempt");
        return Task.CompletedTask;
    }
}