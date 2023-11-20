using System.ComponentModel.DataAnnotations;

namespace Tenor.Models
{
    public class Permission
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 char")]
        public string Name { get; set; }

        public virtual ICollection<RolePermission> RolePermissions { get; set; }

    }
}
