namespace NET_API.Models.CWB;

public class WeatherDataCWBModel
{
  string Success {get; set;}
  WeatherResult Result { get; set;}
  LocationRecord records { get; set;}
}

public class WeatherResult
{
  string Resource_id { get; set;}
  List<WeatherDataField> Fields {get; set;}
}

public class WeatherDataField
{
  string Id {get; set;}
  string Type {get; set;} 
}

public class WeatherRecord
{
  string DatasetDescription {get; set;}
  List<LocationRecord> Location {get; set;}
}

public class LocationRecord
{
  string LocationName {get; set;}
  List<WeatherElement> WeatherElement {get;set;}
}

public class WeatherElement
{
  string ElementName {get; set;}
  List<RecordElementTime> time {get; set;}
}

public class RecordElementTime
{
  string StartTime {get; set;}
  string EndTime {get; set;}
  ParameterItem Parameter {get;set;}
}

public class ParameterItem
{
  string ParameterName {get; set;}
  string ParameterValue {get; set;}
}