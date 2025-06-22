using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace NET_API.Models.TaiPower
{
    [Table("taipower_generation")]
    public class TaiPower
    {
        [Key]
        [Column("time")]
        public DateTime Time {get; set;} 
        [Column("east_generation")]
        public decimal EastGeneration {get; set;}
        [Column("central_generation")]
        public decimal CentralGeneration {get; set;}
        [Column("north_generation")]
        public decimal NorthGeneration {get; set;}
        [Column("south_generation")]
        public decimal SouthGeneration {get; set;}
        [Column("north_consumption")]
        public decimal NorthConsumption {get; set;}
        [Column("south_consumption")]
        public decimal SouthConsumption {get; set;}
        [Column("central_consumption")]
        public decimal CentralConsumption {get; set;}
        [Column("east_consumption")]
        public decimal EastConsumption {get; set;}
        [Column("create_at")]
        public DateTime CreateAt {get; set;}
    }
}