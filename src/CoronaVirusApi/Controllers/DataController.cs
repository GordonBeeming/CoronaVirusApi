using CoronaVirusApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace CoronaVirusApi.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class DataController : ControllerBase
  {
    private readonly DataStorage dataStorage;

    public DataController(DataStorage dataStorage)
    {
      this.dataStorage = dataStorage;
    }

    [HttpGet("raw")]
    public SourceData Raw()
    {
      return dataStorage.GetSourceData();
    }
  }
}
