using System.ComponentModel.DataAnnotations.Schema;

namespace Tenor.Models
{
    public class Device
    {
        public int Id { get; set; }
        public string SupplierId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }


        public virtual ICollection<Device> Childs { get; set; }
        public virtual ICollection<Subset> Subsets { get; set; }

        [ForeignKey("Parent")]
        public int? ParentId { get; set; }
        public virtual Device Parent { get; set; }
    }
}