using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Tenor.Models
{
    public class DashboardField
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("ExtraField")]
        public int FieldId { get; set; }
        public bool IsActive { get; set; }

        public virtual ExtraField ExtraField { get; set; }
        public virtual ICollection<DashboardFieldValue> DashboardFieldValues { get; set; }

        public DashboardField() { }
        public DashboardField(int fieldId)
        {
            FieldId = fieldId;
            IsActive = true;
        }
    }
}
