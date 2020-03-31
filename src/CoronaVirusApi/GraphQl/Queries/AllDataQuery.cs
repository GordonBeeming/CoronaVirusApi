using System;
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

      Field<BucketType>
          ("bucket",
            arguments: new QueryArguments(new
                QueryArgument<IntGraphType>
            { Name = "id" }),
                resolve:
                   context => dataStorage.GetBucket(context.GetArgument<int>("id")));

      Field<ListGraphType<CountryType>>
          ("countries",
            resolve: context => dataStorage.GetAllCountries());

      Field<CountryType>
          ("country",
            arguments: new QueryArguments(new
                QueryArgument<StringGraphType>
            { Name = "geoId" }),
                resolve:
                   context => dataStorage.GetCountryByGeoId(context.GetArgument<string>("geoId")));

      Field<ListGraphType<CountryRecordType>>
          ("records",
            resolve: context => dataStorage.GetAllCountryRecords());

      Field<CountryRecordType>
          ("record",
            arguments: new QueryArguments(
              new QueryArgument<StringGraphType> { Name = "geoId" },
              new QueryArgument<DateTimeGraphType> { Name = "date" }
            ),
            resolve: context => dataStorage.GetCountryRecordByGeoIdAndDate(
              context.GetArgument<string>("geoId"),
              context.GetArgument<DateTime>("date"))
            );
    }
  }
}
