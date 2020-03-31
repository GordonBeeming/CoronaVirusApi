using CoronaVirusApi.Models;
using GraphQL.Types;

namespace CoronaVirusApi.GraphQl.Types
{
  public class CountryType : ObjectGraphType<Country>
  {
    public CountryType()
    {
      Field(o => o.GeoId).Name("Id").Description("The GEO Id is a code for the country. e.g.: ZA");
      Field(o => o.CountriesAndTerritories).Name("Name").Description("The Countries and Territories is a full display name for the country. e.g.: South Africa");
      Field(o => o.FocusCountry).Description($"If cases or deaths is above a threshold we flag the country as focus. Cases ({Constants.FocusCountryCases}), Deaths ({Constants.FocusCountryDeaths})");
      Field(o => o.PopData2018).Name("Population").Description("Population Data as of 2018");
      Field(o => o.CasesBucket, type: typeof(BucketType)).Description("Bucket for the cases to date");
      Field(o => o.DeathsBucket, type: typeof(BucketType)).Description("Bucket for the deaths to date");
      Field(x => x.Records, type: typeof(ListGraphType<CountryRecordType>)).Description("Records");
    }
  }
}
