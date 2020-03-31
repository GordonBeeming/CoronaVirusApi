using CoronaVirusApi.GraphQl.Types;
using GraphQL.Types;

namespace CoronaVirusApi.GraphQl.Queries
{
  public class AllDataQuery : ObjectGraphType
  {
    private readonly DataStorage dataStorage;

    public AllDataQuery(DataStorage dataStorage)
    {
      this.dataStorage = dataStorage;

      Field<ListGraphType<BucketType>>
          ("buckets",
            resolve: context => dataStorage.GetAllBuckets());

      Field<ListGraphType<CountryType>>
          ("countries",
            resolve: context => dataStorage.GetAllCountries());

      Field<CountryType>
          ("country",
            arguments: new QueryArguments(new
                QueryArgument<IntGraphType>
            { Name = "geoId" }),
                resolve:
                   context => dataStorage.GetCountryByGeoId(context.GetArgument<string>("geoId")));

      Field<ListGraphType<CountryRecordType>>
          ("records",
            resolve: context => dataStorage.GetAllCountryRecords());
    }
  }
}
