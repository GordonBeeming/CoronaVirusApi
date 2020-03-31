using System.Collections.Generic;

namespace CoronaVirusApi.Models
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
  public class Country
  {
    public string GeoId { get; set; }
    public string CountriesAndTerritories { get; set; }
    public bool FocusCountry { get; set; }
    public int PopData2018 { get; set; }
    public Bucket CasesBucket { get; set; }
    public Bucket DeathsBucket { get; set; }
    public List<CountryRecord> Records { get; set; } = new List<CountryRecord>();
  }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
}
