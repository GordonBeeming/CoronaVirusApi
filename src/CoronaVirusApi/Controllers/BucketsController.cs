using System.Collections.Generic;
using CoronaVirusApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace CoronaVirusApi.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class BucketsController : ControllerBase
  {
    private readonly DataStorage dataStorage;

    public BucketsController(DataStorage dataStorage)
    {
      this.dataStorage = dataStorage;
    }

    [HttpGet]
    public IEnumerable<Bucket> Get()
    {
      return dataStorage.GetBuckets();
    }
  }
}
