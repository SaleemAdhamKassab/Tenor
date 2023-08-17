using System.ComponentModel.DataAnnotations.Schema;

namespace Tenor.Models
{
    public class MainSet
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }

        [ForeignKey("Parent")]
        public int? ParentId { get; set; }
        public virtual MainSet Parent { get; set; }
        public virtual ICollection<MainSet> Childs { get; set; }

        public virtual ICollection<Subset> Subsets { get; set; }
    }
}
