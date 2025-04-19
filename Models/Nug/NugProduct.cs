using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NET_API.Models.Nug
{
  [Table("nug_products")]
  public class NugProduct
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("store_id")]
    public string StoreId { get; set; }

    [Required]
    [StringLength(255)]
    [Column("name")]
    public string Name { get; set; }

    [StringLength(100)]
    [Column("category")]
    public string Category { get; set; }

    [Column("cost", TypeName = "decimal(10,2)")]
    public decimal? Cost { get; set; }

    [Column("price", TypeName = "decimal(10,2)")]
    public decimal? Price { get; set; }

    [StringLength(50)]
    [Column("unit")]
    public string Unit { get; set; }

    [Column("unit_count")]
    public int? UnitCount { get; set; }

    [Column("use_yn")]
    [StringLength(1)]
    public string UseYn { get; set; } = "Y";

    [Column("created_at")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime CreatedAt { get; private set; }

    [Column("updated_at")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime UpdatedAt { get; private set; }

    // 導航屬性 - 關聯到 NugUser
    [ForeignKey("StoreId")]
    public virtual NugStore Store { get; set; }
  }
}