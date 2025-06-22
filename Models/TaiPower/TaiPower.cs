using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace NET_API.Models.TaiPower
{
    [Table("taipower_generation")]
    public class TaiPower
    {
        [Key]
        [Column("time")]
        public DateTime Time { get; set; }
        [Column("east_generation")]
        public double EastGeneration { get; set; }
        [Column("central_generation")]
        public double CentralGeneration { get; set; }
        [Column("north_generation")]
        public double NorthGeneration { get; set; }
        [Column("south_generation")]
        public double SouthGeneration { get; set; }
        [Column("north_consumption")]
        public double NorthConsumption { get; set; }
        [Column("south_consumption")]
        public double SouthConsumption { get; set; }
        [Column("central_consumption")]
        public double CentralConsumption { get; set; }
        [Column("east_consumption")]
        public double EastConsumption { get; set; }
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}