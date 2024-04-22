using System.ComponentModel.DataAnnotations;

namespace Tenor.Models
{
    public class Operator
    {
        [Key]
        public int Id { get; set; }
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 char")]
        [Required]
        public string Name { get; set; }
        public bool IsLogic { get;set; }

        public virtual ICollection<Operation> Operations { get; set; }

        public virtual ICollection<MeasureHaving> MeasureHavings { get; set; }
    }
}