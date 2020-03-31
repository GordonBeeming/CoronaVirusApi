using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoronaVirusApi.BackgroundServices.Config;
using CoronaVirusApi.HttpServices;
using CoronaVirusApi.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace CoronaVirusApi.BackgroundServices
{
  public class UpdateDataBackgroundService : BackgroundService
  {
    private readonly ILogger<UpdateDataBackgroundService> logger;
    private readonly OpenDataHttpService openDataHttpService;
    private readonly DataStorage dataStorage;
    private readonly ServiceConfig config;

    private string? latestJsonData = null;
    private SourceData? latestSourceData = null;

    public UpdateDataBackgroundService(ILogger<UpdateDataBackgroundService> logger,
      IOptions<ServiceConfig> serviceConfig,
      OpenDataHttpService openDataHttpService,
      DataStorage dataStorage)
    {
      this.logger = logger;
      this.openDataHttpService = openDataHttpService;
      this.dataStorage = dataStorage;
      this.config = serviceConfig.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      logger.LogDebug($"UpdateDataBackgroundService is starting.");

      stoppingToken.Register(() => logger.LogDebug($"UpdateDataBackgroundService is stopping."));

      while (!stoppingToken.IsCancellationRequested)
      {
        if (!await Time($"UpdateDataBackgroundService running", async (stoppingToken) =>
        {
          if (!await DownloadNewFile(stoppingToken))
          {
            return false;
          }
          if (!await EnrichData(stoppingToken))
          {
            return false;
          }
          if (!await SaveData(stoppingToken))
          {
            return false;
          }
          return true;
        }, stoppingToken))
        {
          dataStorage.SetLatestLoadError();
        }

        latestSourceData = null;
        latestJsonData = null;

        await Task.Delay(config.UpdateServiceTickInMilliSeconds, stoppingToken);
      }

      logger.LogDebug($"UpdateDataBackgroundService is stopping.");
    }

    private Task<bool> SaveData(CancellationToken stoppingToken)
    {
      return Time("Saving data", async (stoppingToken) =>
      {
        if (latestSourceData != null)
        {
          await dataStorage.SetSourceData(latestSourceData, stoppingToken);
        }
        return true;
      }, stoppingToken);
    }

    private Task<bool> EnrichData(CancellationToken stoppingToken)
    {
      return Time("Enrich data", (stoppingToken) =>
      {
        latestSourceData = null;
        latestSourceData = JsonConvert.DeserializeObject<SourceData>(latestJsonData);
        var geoIds = latestSourceData.Records.Select(o => o.GeoId).Distinct();
        foreach (var geoId in geoIds)
        {
          var recordsInGeo = latestSourceData.Records.Where(o => o.GeoId == geoId).OrderBy(o => o.Date);
          var casesCount = 0;
          var deathsCount = 0;
          DateTime? firstCaseDate = null;
          DateTime? firstDeathDate = null;
          foreach (var record in recordsInGeo)
          {
            record.CountriesAndTerritories = record.CountriesAndTerritories.Replace("_", " ");
            casesCount += record.CasesNumber;
            deathsCount += record.DeathsNumber;
            record.CasesToDate = casesCount;
            record.DeathsToDate = deathsCount;

            if (!firstCaseDate.HasValue && casesCount > 0)
            {
              firstCaseDate = record.Date;
            }
            if (firstCaseDate.HasValue)
            {
              record.DaysWithCases = Convert.ToInt32((record.Date - firstCaseDate.Value).TotalDays) + 1;
            }
            if (!firstDeathDate.HasValue && deathsCount > 0)
            {
              firstDeathDate = record.Date;
            }
            if (firstDeathDate.HasValue)
            {
              record.DaysWithDeaths = Convert.ToInt32((record.Date - firstDeathDate.Value).TotalDays) + 1;
            }
          }
          foreach (var record in recordsInGeo)
          {
            record.FocusCountry = casesCount >= Constants.FocusCountryCases || deathsCount >= Constants.FocusCountryDeaths;
            record.CasesBucket = GetBucketName(casesCount);
            record.DeathsBucket = GetBucketName(deathsCount);
          }
        }
        return Task.FromResult(true);
      }, stoppingToken);
    }

    private Task<bool> DownloadNewFile(CancellationToken stoppingToken)
    {
      return Time("Downloading new data file", async (stoppingToken) =>
      {
        latestJsonData = null;
        latestJsonData = await openDataHttpService.GetDataJson();
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
