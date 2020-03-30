using System.Collections.Generic;
using System.Threading.Tasks;
using CoronaVirusApi.Models;

namespace CoronaVirusApi
{
  public static class DataStorage
  {
    static DataStorage()
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
    }

    private static List<Bucket> buckets;
    public static List<Bucket> GetBuckets() => buckets;

    private static SourceData sourceData;
    public static SourceData GetSourceData() => sourceData;
    public static Task SetSourceData(SourceData data)
    {
      sourceData = data;
      return Task.CompletedTask;
    }
  }
}
