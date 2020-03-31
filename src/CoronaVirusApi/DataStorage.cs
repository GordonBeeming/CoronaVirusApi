using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoronaVirusApi.Config;
using CoronaVirusApi.Models;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace CoronaVirusApi
{
  public class DataStorage
  {
    private readonly List<Bucket> buckets;
    private List<Country> countries = new List<Country>();
    private string sourceDataJson;

    private readonly AzureStorageConfig azureStorageConfig;
    private readonly ILogger<DataStorage> logger;

    public DataStorage(ILogger<DataStorage> logger, IOptions<AzureStorageConfig> azureStorageConfig)
    {
      buckets = new List<Bucket>
      {
        new Bucket(1,"< 249",0, 249),
        new Bucket(2,"250 - 499",250, 499),
        new Bucket(3,"500 - 999",500, 999),
        new Bucket(4,"1000 - 4999",1000, 4999),
        new Bucket(5,"5000 - 24999",5000, 24999),
        new Bucket(6,"> 25000",25000, null),
      };
      sourceDataJson = JsonConvert.SerializeObject(new SourceData());
      this.azureStorageConfig = azureStorageConfig.Value;
      this.logger = logger;

      LoadSourceDataFromDisk();
    }

    #region Graph QL

    public List<Bucket> GetAllBuckets() => buckets;

    public List<Country> GetAllCountries() => countries;

    public Country GetCountryByGeoId(string geoId) => countries.FirstOrDefault(o => o.GeoId.Equals(geoId, StringComparison.InvariantCultureIgnoreCase));

    public IEnumerable<CountryRecord> GetAllCountryRecords() => countries.SelectMany(o => o.Records);

    #endregion

    public List<Bucket> GetBuckets() => buckets;

    public string GetSourceDataRaw() => sourceDataJson;
    public async Task SetSourceData(SourceData data, CancellationToken stoppingToken)
    {
      var jsonString = JsonConvert.SerializeObject(data);

      var container = await GetContainer();

      await UploadJson(jsonString, $@"archive\{data.LastUpdate:yyyyMMdd-HHmmss}.json", container, stoppingToken);
      await UploadJson(jsonString, "latest.json", container, stoppingToken);

      sourceDataJson = jsonString;

      await UpdateGraphData(data, stoppingToken);
    }

    public Task UpdateGraphData(SourceData data, CancellationToken stoppingToken)
    {
      var countries = new List<Country>();
      var sourceData = JsonConvert.DeserializeObject<SourceData>(sourceDataJson);
      var geoIds = sourceData.Records.Select(o => o.GeoId.ToLowerInvariant()).Distinct();
      foreach (var geoId in geoIds)
      {
        var firstMatch = sourceData.Records.FirstOrDefault(o => o.GeoId.Equals(geoId, StringComparison.InvariantCultureIgnoreCase));
        var country = new Country
        {
          GeoId = firstMatch.GeoId,
          CasesBucket = buckets.FirstOrDefault(oo => oo.Name == firstMatch.CasesBucket),
          DeathsBucket = buckets.FirstOrDefault(oo => oo.Name == firstMatch.DeathsBucket),
          CountriesAndTerritories = firstMatch.CountriesAndTerritories,
          FocusCountry = firstMatch.FocusCountry,
          PopData2018 = firstMatch.PopData2018?.Length > 0 ? Convert.ToInt32(firstMatch.PopData2018) : 0,
        };
        countries.Add(country);
        var records = sourceData.Records.Where(o => o.GeoId.Equals(firstMatch.GeoId, StringComparison.InvariantCultureIgnoreCase));
        foreach (var record in records)
        {
          var countryRecord = new CountryRecord
          {
            GeoId = firstMatch.GeoId,
            Date = record.Date,
            Day = Convert.ToInt32(record.Day),
            Month = Convert.ToInt32(record.Month),
            Year = Convert.ToInt32(record.Year),
            Cases = record.CasesNumber,
            Deaths = record.DeathsNumber,
            CasesToDate = record.CasesToDate,
            DeathsToDate = record.DeathsToDate,
            DaysWithCases = record.DaysWithCases,
            DaysWithDeaths = record.DaysWithDeaths,
          };
          country.Records.Add(countryRecord);
        }
      }
      this.countries = countries;
      return Task.CompletedTask;
    }

    public void SetLatestLoadError()
    {
      if (sourceDataJson != null)
      {
        var sourceData = JsonConvert.DeserializeObject<SourceData>(sourceDataJson);
        sourceData.UpdateError = true;
        sourceDataJson = JsonConvert.SerializeObject(sourceData);
      }
    }

    private async void LoadSourceDataFromDisk()
    {
      var container = await GetContainer();

      var sourceDataJson = await DownloadJson("latest.json", container);
      if (sourceDataJson == null)
      {
        return;
      }
      try
      {
        SourceData sourceData = JsonConvert.DeserializeObject<SourceData>(sourceDataJson);
        sourceData.UpdateError = false;
        this.sourceDataJson = JsonConvert.SerializeObject(sourceData);

        await UpdateGraphData(sourceData, CancellationToken.None);
      }
      catch (Exception ex)
      {
        SetLatestLoadError();
        logger.LogError(ex, "Error deserializing latest json source data from storage");
      }
    }

    private async Task UploadJson(string jsonString, string filePath, CloudBlobContainer container, CancellationToken stoppingToken)
    {
      var latestBlob = container.GetBlockBlobReference($@"json\{filePath}");
      latestBlob.Properties.ContentDisposition = $"inline; filename=\"{filePath}\"";
      latestBlob.Properties.ContentType = "application/json";
      await latestBlob.UploadTextAsync(jsonString, stoppingToken);
    }

    private async Task<string?> DownloadJson(string filePath, CloudBlobContainer container)
    {
      var latestBlob = container.GetBlockBlobReference($@"json\{filePath}");
      if (await latestBlob.ExistsAsync())
      {
        return await latestBlob.DownloadTextAsync();
      }
      return null;
    }

    private async Task<CloudBlobContainer> GetContainer()
    {
      var storageAccount = CloudStorageAccount.Parse(azureStorageConfig.ConnectionString);
      var cloudBlobClient = storageAccount.CreateCloudBlobClient();
      var container = cloudBlobClient.GetContainerReference("corona-virus");
      await container.CreateIfNotExistsAsync();
      return container;
    }
  }
}
