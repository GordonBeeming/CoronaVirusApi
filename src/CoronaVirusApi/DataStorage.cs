using System;
using System.Collections.Generic;
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
    private SourceData sourceData;
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
      sourceData = new SourceData(); // change to read from blob storage
      this.azureStorageConfig = azureStorageConfig.Value;
      this.logger = logger;

      LoadSourceDataFromDisk();
    }

    public List<Bucket> GetBuckets() => buckets;

    public SourceData GetSourceData() => sourceData;
    public async Task SetSourceData(SourceData data, CancellationToken stoppingToken)
    {
      await SaveSourceDataToDisk(data, stoppingToken);
      sourceData = data;
    }

    public void SetLatestLoadError()
    {
      if (sourceData != null)
      {
        sourceData.UpdateError = true;
      }
    }

    private async void LoadSourceDataFromDisk()
    {
      var container = await GetContainer();

      var jsonString = await DownloadJson("latest.json", container);
      if (jsonString == null)
      {
        return;
      }
      try
      {
        SourceData data = JsonConvert.DeserializeObject<SourceData>(jsonString);
        data.UpdateError = false;
        sourceData = data;
      }
      catch (Exception ex)
      {
        SetLatestLoadError();
        logger.LogError(ex, "Error deserializing latest json source data from storage");
      }
    }

    private async Task SaveSourceDataToDisk(SourceData data, CancellationToken stoppingToken)
    {
      var jsonString = JsonConvert.SerializeObject(data);

      var container = await GetContainer();

      await UploadJson(jsonString, $@"archive\{data.LastUpdate:yyyyMMdd-HHmmss}.json", container, stoppingToken);
      await UploadJson(jsonString, "latest.json", container, stoppingToken);
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
