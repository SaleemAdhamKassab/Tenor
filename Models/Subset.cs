using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tenor.Models
{
    public class Subset
    {
        public int Id { get; set; }
        [Required]
        public string SupplierId { get; set; }
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 char")]
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        public string TableName { get; set; }
        public string RefTableName { get; set; }
        public string SchemaName { get; set; }
        public string Schema { get; set; }
        public string RefSchema { get; set; }
        public int? MaxDataDate { get; set; }
        public bool IsLoad { get; set; }
        public string DataTS { get; set; }
        public string IndexTS { get; set; }
        public string DbLink { get; set; }
        public string RefDbLink { get; set; }
        [ForeignKey("Device")]
        public int DeviceId { get; set; }
        public bool IsDeleted { get; set; }


        public virtual Device Device { get; set; }
        public virtual ICollection<Counter> Counters { get; set; }    
    }
}