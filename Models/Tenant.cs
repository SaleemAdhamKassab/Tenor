using System.ComponentModel.DataAnnotations;

namespace Tenor.Models
{
    public class Tenant
    {
        public int Id { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 char")]
        public string Name { get; set; }
        public virtual ICollection<GroupTenantRole> GroupTenantRoles { get; set; }
        public virtual ICollection<UserTenantRole> UserTenantRoles { get; set; }
        public virtual ICollection<TenantDevice> TenantDevices { get; set; }

    }
}