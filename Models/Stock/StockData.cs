
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NET_API.Models.Stock;

[Table("stock_data")]
public class StockData
{
  [Key]
  [Column("id")]
  public int Id {get; set;}
  [Column("price")]
  public decimal Price {get; set;}
  [Column("timestamp")]
  public DateTime Timestamp {get; set;}
  [Column("symbol")]
  public string Symbol {get; set;}
}