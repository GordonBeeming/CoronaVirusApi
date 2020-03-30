using CoronaVirusApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace CoronaVirusApi.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class DataController : ControllerBase
  {
    public DataController()
    {
    }

    [HttpGet("raw")]
    public SourceData Raw()
    {
      return DataStorage.GetSourceData();
    }
  }
}
