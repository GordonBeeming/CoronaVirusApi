namespace CoronaVirusApi.BackgroundServices.Config
{
  public class ServiceConfig
  {
    public int UpdateServiceTickInMinutes { get; set; }

    internal int UpdateServiceTickInMilliSeconds => UpdateServiceTickInMinutes * 60 * 1000;
  }
}
