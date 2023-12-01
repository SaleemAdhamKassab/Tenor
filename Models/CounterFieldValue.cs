using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Tenor.Models
{
    public class CounterFieldValue
    {

        [Key]
        public int Id { get; set; }
        [ForeignKey("Counter")]
        public int CounterId { get; set; }
        [ForeignKey("CounterField")]
        public int CounterFieldId { get; set; }
        public string? FieldValue { get; set; }

        public virtual Counter  Counter { get; set; }
        public virtual CounterField  CounterField { get; set; }
    }
}
