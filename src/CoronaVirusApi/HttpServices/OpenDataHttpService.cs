using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CoronaVirusApi.HttpServices
{
  public class OpenDataHttpService
  {
    private const string JsonDownloadPath = "https://opendata.ecdc.europa.eu/covid19/casedistribution/json/";

    private readonly HttpClient httpClient;

    public OpenDataHttpService(HttpClient httpClient)
    {
      this.httpClient = httpClient;
    }

    public async Task<string> GetDataJson()
    {
      var responseString = await httpClient.GetStringAsync(JsonDownloadPath);
      var responseStringLines = responseString.Split('\n');
      var responseJson = string.Join(Environment.NewLine, responseStringLines.Where(o =>
         o.Trim().StartsWith('[') ||
         o.Trim().StartsWith('{') ||
         o.Trim().StartsWith('\"') ||
         o.Trim().StartsWith('}') ||
         o.Trim().StartsWith(']')
      ));
      return responseJson;
    }
  }
}
