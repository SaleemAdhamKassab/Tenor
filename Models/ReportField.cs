using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Tenor.Models
{
    public class ReportField
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("ExtraField")]
        public int FieldId { get; set; }
        public bool IsActive { get; set; }

        public virtual ExtraField ExtraField { get; set; }
        public virtual ICollection<ReportFieldValue> ReportFieldValues { get; set; }

    }
}
