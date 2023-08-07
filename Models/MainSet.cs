using System.ComponentModel.DataAnnotations.Schema;

namespace Tenor.Models
{
    public class MainSet
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
        public int? ParentId { get; set; }



        public virtual List<Subset> Subsets { get; set; }

        public int TenantId { get; set; }
        public virtual Tenant Tenant { get; set; }
    }
}
