namespace CoronaVirusApi.Controllers
{
  //[Route(Startup.GraphQlPath)]
  //public class GraphQlController : ControllerBase
  //{
  //  private readonly DataStorage dataStorage;

  //  public GraphQlController(DataStorage dataStorage)
  //  {
  //    this.dataStorage = dataStorage;
  //  }

  //  [HttpPost]
  //  public async Task<IActionResult> Post([FromBody] GraphQlQuery query)
  //  {
  //    var schema = new Schema { Query = new AllDataQuery(dataStorage) };

  //    var result = await new DocumentExecuter().ExecuteAsync(x =>
  //    {
  //      x.Schema = schema;
  //      x.Query = query.Query;
  //      x.Inputs = query.Variables;
  //    });

  //    if (result.Errors?.Count > 0)
  //    {
  //      return BadRequest();
  //    }

  //    return Ok(result);
  //  }
  //}

}
