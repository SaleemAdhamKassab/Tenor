using System.ComponentModel.DataAnnotations;

namespace Tenor.Models
{
    public class Function
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int ArgumentsCount { get; set; }
        public bool IsBool { get; set; }


        public virtual ICollection<Operation> Operations { get; set; }
    }
}
