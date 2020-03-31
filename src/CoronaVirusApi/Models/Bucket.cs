namespace CoronaVirusApi.Models
{
  public class Bucket
  {
    public Bucket(int id, string name, int min, int? max)
    {
      Id = id;
      Name = name;
      Min = min;
      Max = max;
    }

    public int Id { get; }

    public string Name { get; }

    public int Min { get; }

    public int? Max { get; }

    public bool IsNumberMatch(int value) => Min <= value && (Max.HasValue ? (value <= Max.Value) : true);
  }
}
