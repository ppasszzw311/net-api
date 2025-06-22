namespace NET_API.Dtos.TaiPower;

public class PowerData
{
    public DateTime Time {get; set;}
    public decimal CentralConsumption { get; set;}
    public decimal NorthConsumption { get; set;}
    public decimal SouthConsumption {get; set;}
    public decimal EastConsumption {get; set;}
}

public class PowerDataResponse
{
  public List<PowerData> Data {get; set;} = new List<PowerData>();
  public int Count {get; set;} = 0;
}