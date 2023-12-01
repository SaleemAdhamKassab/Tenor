using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Tenor.Models
{
    public class KpiFieldValue
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Kpi")]
        public int KpiId { get; set; }
        [ForeignKey("KpiField")]
        public int KpiFieldId { get; set; }
        public string ? FieldValue { get; set; }
         
        public virtual Kpi Kpi { get; set; }
        public virtual KpiField  KpiField { get; set; }
    }
}
