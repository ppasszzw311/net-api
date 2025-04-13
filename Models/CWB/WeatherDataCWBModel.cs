namespace NET_API.Models.CWB;

public class WeatherDataCWBModel
{
  public string Success {get; set;}
  public WeatherResult Result { get; set;}
  public LocationRecord records { get; set;}
}

public class WeatherResult
{
  public string Resource_id { get; set;}
  public List<WeatherDataField> Fields {get; set;}
}

public class WeatherDataField
{
  public string Id {get; set;}
  public string Type {get; set;} 
}

public class WeatherRecord
{
  public string DatasetDescription {get; set;}
  public List<LocationRecord> Location {get; set;}
}

public class LocationRecord
{
  public string LocationName {get; set;}
  public List<WeatherElement> WeatherElement {get;set;}
}

public class WeatherElement
{
  public string ElementName {get; set;}
  public List<RecordElementTime> time {get; set;}
}

public class RecordElementTime
{
  public string StartTime {get; set;}
  public string EndTime {get; set;}
  public ParameterItem Parameter {get;set;}
}

public class ParameterItem
{
  public string ParameterName {get; set;}
  public string ParameterValue {get; set;}
}