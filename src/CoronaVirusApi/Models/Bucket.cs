namespace CoronaVirusApi.Models
{
  public class Bucket
  {
    public Bucket(byte id, string name, int min, int? max)
    {
      Id = id;
      Name = name;
      Min = min;
      Max = max;
    }

    public byte Id { get; }

    public string Name { get; }

    public int Min { get; }

    public int? Max { get; }

    public bool IsNumberMatch(int value) => Min <= value && (Max.HasValue ? (value >= Max.Value) : true);
  }
}
