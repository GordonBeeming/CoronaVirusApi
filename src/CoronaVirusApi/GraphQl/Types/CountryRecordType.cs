using CoronaVirusApi.Models;
using GraphQL.Types;

namespace CoronaVirusApi.GraphQl.Types
{
  public class CountryRecordType : ObjectGraphType<CountryRecord>
  {
    public CountryRecordType()
    {
      Field(o => o.GeoId);
      Field(o => o.Date);
      Field(o => o.Day);
      Field(o => o.Month);
      Field(o => o.Year);
      Field(o => o.Cases);
      Field(o => o.Deaths);
      Field(o => o.CasesToDate);
      Field(o => o.DeathsToDate);
      Field(o => o.DaysWithCases);
      Field(o => o.DaysWithDeaths);
    }
  }
}
