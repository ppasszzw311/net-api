using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NET_API.Models.Nug
{
  [Table("nug_store")]
  public class NugStore
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("uuid")]
    public Guid UUID { get; private set; }

    [Required]
    [Column("owner_id")]
    public string OwnerId { get; set; }  // 注意這裡的類型改為 string，對應 NugUser.Id

    [Required]
    [StringLength(100)]
    [Column("store_name")]
    public string StoreName { get; set; }

    [StringLength(50)]
    [Column("store_type")]
    public string StoreType { get; set; }

    [StringLength(20)]
    [Column("phone")]
    public string Phone { get; set; }

    [Column("address")]
    public string Address { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime CreatedAt { get; private set; }

    [Column("updated_at")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime UpdatedAt { get; private set; }

    // 導航屬性 - 關聯到 NugUser
    [ForeignKey("OwnerId")]
    public virtual NugUser Owner { get; set; }
  }
}