using Hangfire;
using Hangfire.Server;
using HangfirePoc.Services;

namespace HangfirePoc.Jobs;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class TaskExecutor(IRandomTextGenerator textGenerator, ILogger<TaskExecutor> logger)
{
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = [5, 10])]
    public Task ExecuteTask1(Guid jobId)
    {
        logger.LogInformation("Task1 - JobId: {JobId}", jobId);
        logger.LogInformation("Random text: {Text}", textGenerator.Generate());
        return Task.CompletedTask;
    }

    [AutomaticRetry(Attempts = 3, DelaysInSeconds = [5, 10])]
    public Task ExecuteTask2(Guid jobId)
    {
        logger.LogInformation("Task2 - JobId: {JobId}", jobId);
        return Task.CompletedTask;
    }

    [AutomaticRetry(Attempts = 3, DelaysInSeconds = [5, 10])]
    public Task ExecuteTask3(PerformContext? context, Guid jobId)
    {
        int? retryCount = context?.GetJobParameter<int>("RetryCount");

        throw new InvalidOperationException($"Retry count is {retryCount} for job {jobId}");
    }
}