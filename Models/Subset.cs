using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tenor.Models
{
    public class Subset
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string SupplierId { get; set; }

        [Required, StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 char")]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public string TableName { get; set; }

        public string RefTableName { get; set; }
        public string SchemaName { get; set; }
        public string RefSchema { get; set; }
        public int? MaxDataDate { get; set; }
        public bool IsLoad { get; set; } = false;
        public string DataTS { get; set; }
        public string IndexTS { get; set; }
        public string DbLink { get; set; }
        public string RefDbLink { get; set; }
        public int? GranularityPeriod { get; set; }
        public string DimensionTable { get; set; }
        public string JoinExpression { get; set; }
        public char? StartChar { get; set; }
        public string? FactDimensionReference { get; set; }
        public int? LoadPriorety { get; set; }
        public string? SummaryType { get; set; }
        public bool IsDeleted { get; set; }

        public int? SetId { get; set; }
        public string? SetName { get; set; }

        [ForeignKey("Device")]
        public int DeviceId { get; set; }

        
        public virtual Device Device { get; set; }
        public virtual ICollection<Counter> Counters { get; set; }
        public virtual ICollection<SubsetFieldValue> SubsetFieldValues { get; set; }
    }
}