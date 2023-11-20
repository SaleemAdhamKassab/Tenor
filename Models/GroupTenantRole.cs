using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tenor.Models
{
    public class GroupTenantRole
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 char")]
        public string GroupName { get; set; }
        [Required]
        [ForeignKey("Tenant")]
        public int TenantId { get; set; }
        [Required]
        [ForeignKey("Role")]
        public int RoleId { get; set; }

        public virtual Role Role { get; set; }
        public virtual Tenant Tenant { get; set; }
    }
}
