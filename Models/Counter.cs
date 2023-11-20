using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tenor.Models
{
    public class Counter
    {
        [Key]
        public int Id { get; set; }
        public string SupplierId { get; set; }
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 char")]
        [Required]
        public string Name { get; set; }
        public string Code { get; set; }
        public string ColumnName { get; set; }
        public string RefColumnName { get; set; }
        public string Description { get; set; }
        public string Aggregation { get; set; }
        [ForeignKey("Subset")]
        public int SubsetId { get; set; }
        public bool IsDeleted { get; set; }

        public virtual Collection<Operation> Operations { get; set; }       
        public virtual Subset Subset { get; set; }
    }
}