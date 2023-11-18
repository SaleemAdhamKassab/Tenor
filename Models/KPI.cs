using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tenor.Models
{
    public class Kpi
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        [ForeignKey("Operation")]
        public int OperationId { get; set; }


        
        public virtual Operation Operation { get; set; }
    }
}