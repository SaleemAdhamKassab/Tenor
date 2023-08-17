using System.ComponentModel.DataAnnotations;

namespace Tenor.Models
{
    public class Operator
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }


        public virtual ICollection<Operation> Operations { get; set; }
    }
}
