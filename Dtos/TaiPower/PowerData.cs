namespace NET_API.Dtos.TaiPower;

public class PowerData
{
  public DateTime Time { get; set; }
  public double? CentralConsumption { get; set; }
  public double? NorthConsumption { get; set; }
  public double? SouthConsumption { get; set; }
  public double? EastConsumption { get; set; }
}

public class PowerDataResponse
{
  public List<PowerData> Data { get; set; } = new List<PowerData>();
  public int Count { get; set; } = 0;
}