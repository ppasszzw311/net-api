using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NET_API.Models.Nug;

[Table("nug_users")]
public class NugUser
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("uuid")]
    public Guid UUID { get; private set; }

    [Required]
    [Column("id")]
    [StringLength(50)]
    public string Id { get; set; }

    [Required]
    [Column("password")]
    public string Password { get; set; }

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; }

    [Column("phone")]
    [StringLength(20)]
    public string Phone { get; set; }

    [Column("address")]
    [StringLength(255)]
    public string Address { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime CreateAt { get; private set; }

    [Column("updated_at")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime UpdateAt { get; private set; }
}
