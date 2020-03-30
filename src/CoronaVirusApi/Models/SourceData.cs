using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CoronaVirusApi.Models
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
  public class SourceData
  {
    public List<Record> Records { get; set; }

    public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
  }

  public class Record
  {
    public string DateRep { get; set; }
    public string Day { get; set; }
    public string Month { get; set; }
    public string Year { get; set; }
    public string Cases { get; set; }
    public string Deaths { get; set; }
    public string CountriesAndTerritories { get; set; }
    public string GeoId { get; set; }
    public string CountryCode { get; set; }
    public string PopData2018 { get; set; }
    public int CasesToDate { get; set; }
    public int DeathsToDate { get; set; }
    public int DaysWithCases { get; set; }
    public int DaysWithDeaths { get; set; }
    public bool FocusCountry { get; set; }
    public string CasesBucket { get; set; }
    public string DeathsBucket { get; set; }

    [JsonIgnore]
    public DateTime Date => DateTime.ParseExact(DateRep, "dd/MM/yyyy", null);
    [JsonIgnore]
    internal int CasesNumber => Convert.ToInt32(Cases);
    [JsonIgnore]
    internal int DeathsNumber => Convert.ToInt32(Deaths);
  }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
}
