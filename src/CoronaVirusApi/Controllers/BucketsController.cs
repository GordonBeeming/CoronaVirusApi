using System.Collections.Generic;
using CoronaVirusApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace CoronaVirusApi.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class BucketsController : ControllerBase
  {
    [HttpGet]
    public IEnumerable<Bucket> Get()
    {
      return DataStorage.GetBuckets();
    }
  }
}
