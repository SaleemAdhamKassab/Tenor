using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tenor.Models
{
    public class Counter
    {
        public int Id { get; set; }


        [Required]
        public string SupplierId { get; set; }


        [Required, StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 char")]
        public string Name { get; set; }

        public string? Code { get; set; }
        public string ColumnName { get; set; }
        public string RefColumnName { get; set; }
        public string Description { get; set; }
        public string Aggregation { get; set; }
        public bool IsDeleted { get; set; }

        [ForeignKey("Subset")]
        public int SubsetId { get; set; }
        public virtual Subset Subset { get; set; }

        public virtual ICollection<Operation> Operations { get; set; }
        public virtual ICollection<CounterFieldValue> CounterFieldValues { get; set; }
    }
}