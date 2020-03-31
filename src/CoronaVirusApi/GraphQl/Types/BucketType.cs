using CoronaVirusApi.Models;
using GraphQL.Types;

namespace CoronaVirusApi.GraphQl.Types
{
  public class BucketType : ObjectGraphType<Bucket>
  {
    public BucketType()
    {
      Field(o => o.Id);
      Field(o => o.Name);
      Field(o => o.Min);
      Field(o => o.Max, nullable: true);
    }
  }
}
