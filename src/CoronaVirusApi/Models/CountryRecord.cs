using System;

namespace CoronaVirusApi.Models
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
  public class CountryRecord
  {
    public string GeoId { get; set; }
    public DateTime Date { get; set; }
    public int Day { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public int Cases { get; set; }
    public int Deaths { get; set; }
    public int CasesToDate { get; set; }
    public int DeathsToDate { get; set; }
    public int DaysWithCases { get; set; }
    public int DaysWithDeaths { get; set; }
  }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
}
