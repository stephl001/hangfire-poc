using Hangfire;
using Hangfire.States;

namespace HangfirePoc.Jobs;

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