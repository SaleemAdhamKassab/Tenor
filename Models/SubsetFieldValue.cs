using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Tenor.Models
{
    public class SubsetFieldValue
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Subset")]
        public int SubsetId { get; set; }
        [ForeignKey("SubsetField")]
        public int SubsetFieldId { get; set; }
        public string? FieldValue { get; set; }

        public virtual Subset Subset { get; set; }
        public virtual SubsetField SubsetField { get; set; }
    }
}
