using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tenor.Models
{
    public class Kpi
    {
        [Key]
        public int Id { get; set; }
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 char")]
        [Required]
        public string Name { get; set; }
        [ForeignKey("Operation")]
        public int OperationId { get; set; }


        
        public virtual Operation Operation { get; set; }
    }
}