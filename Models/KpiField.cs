using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tenor.Models
{
    public class KpiField
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("ExtraField")]
        public int FieldId { get;set; }
        public bool IsActive { get; set; }

        public virtual ExtraField ExtraField { get; set; }
        public virtual ICollection<KpiFieldValue> KpiFieldValues { get; set; }

        public KpiField() {}
        public KpiField(int fieldId)
        {
            FieldId=fieldId;
            IsActive = true;
        }
    }
}
