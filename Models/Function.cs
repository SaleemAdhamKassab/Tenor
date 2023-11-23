using System.ComponentModel.DataAnnotations;

namespace Tenor.Models
{
    public class Function
    {
        [Key]
        public int Id { get; set; }
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 char")]
        [Required]
        public string Name { get; set; }
        public int ArgumentsCount { get; set; }
        public bool IsBool { get; set; }


        public virtual ICollection<Operation> Operations { get; set; }
    }
}
