using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tenor.Models
{
    public class MainSet
    {
        [Key]
        public int Id { get; set; }
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 char")]
        [Required] public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        public string SupplierId { get; set; } //set Id from Hawawi
        [ForeignKey("Parent")]
        public int? ParentId { get; set; }
        public bool IsDeleted { get; set; } //Without Gloable filter


        public virtual ICollection<MainSet> Childs { get; set; }
        public virtual ICollection<Subset> Subsets { get; set; }      
        public virtual MainSet Parent { get; set; }//Self Join Relation
        
    }
}