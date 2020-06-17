using System;
using System.Threading;
using System.Threading.Tasks;
using CoronaVirusApi.BackgroundServices.Config;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoronaVirusApi.BackgroundServices
{
  public class CleanupStorageBackgroundService : BackgroundService
  {
    private readonly ILogger<CleanupStorageBackgroundService> logger;
    private readonly DataStorage dataStorage;
    private readonly ServiceConfig config;

    public CleanupStorageBackgroundService(ILogger<CleanupStorageBackgroundService> logger,
      IOptions<ServiceConfig> serviceConfig,
      DataStorage dataStorage)
    {
      this.logger = logger;
      this.dataStorage = dataStorage;
      this.config = serviceConfig.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      logger.LogDebug($"CleanupStorageBackgroundService is starting.");

      stoppingToken.Register(() => logger.LogDebug($"CleanupStorageBackgroundService is stopping."));

      while (!stoppingToken.IsCancellationRequested)
      {
        if (!await Time($"CleanupStorageBackgroundService running", async (stoppingToken) =>
        {
          if (!await CleanDataFromStorage(stoppingToken))
          {
            return false;
          }
          return true;
        }, stoppingToken))
        {
          dataStorage.SetLatestLoadError();
        }

        await Task.Delay(config.UpdateServiceTickInMilliSeconds, stoppingToken);
      }

      logger.LogDebug($"CleanupStorageBackgroundService is stopping.");
    }

    private Task<bool> CleanDataFromStorage(CancellationToken stoppingToken)
    {
      return Time("Saving data", async (stoppingToken) =>
      {
        await dataStorage.Clean(stoppingToken);
        return true;
      }, stoppingToken);
    }

    private async Task<bool> Time(string message, Func<CancellationToken, Task<bool>> work, CancellationToken stoppingToken)
    {
      logger.LogDebug($"<started> {message}");
      DateTime startTime = DateTime.UtcNow;
      bool result = true;
      bool error = false;
      try
      {
        result = await work(stoppingToken);
      }
      catch (Exception ex)
      {
        logger.LogError(ex, $"Unknown error in {message}");
        error = true;
      }
      DateTime endTime = DateTime.UtcNow;
      var totalSeconds = (endTime - startTime).TotalSeconds;
      string infoTimeTaken;
      if (totalSeconds == 0)
      {
        infoTimeTaken = "less than a second";
      }
      else if (totalSeconds == 1)
      {
        infoTimeTaken = "a second";
      }
      else
      {
        infoTimeTaken = $"{totalSeconds} seconds";
      }
      logger.LogDebug($"<finished> {message} ({infoTimeTaken}){(error ? " with error" : string.Empty)}");
      return result && !error;
    }

    private string GetBucketName(int value)
    {
      foreach (var bucket in dataStorage.GetBuckets())
      {
        if (bucket.IsNumberMatch(value))
        {
          return bucket.Name;
        }
      }
      return string.Empty;
    }
  }
}
