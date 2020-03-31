using CoronaVirusApi.GraphQl.Queries;
using GraphQL;
using GraphQL.Types;

namespace CoronaVirusApi.GraphQl.Schemas
{
  public class AllDataSchema : Schema
  {
    public AllDataSchema(IDependencyResolver resolver) : base(resolver)
    {
      Query = resolver.Resolve<AllDataQuery>();
    }
  }
}
