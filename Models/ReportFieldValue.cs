using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Tenor.Models
{
    public class ReportFieldValue
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Report")]
        public int ReportId { get; set; }
        [ForeignKey("ReportField")]
        public int ReportFieldId { get; set; }
        public string? FieldValue { get; set; }

        public virtual Report Report { get; set; }
        public virtual ReportField ReportField { get; set; }
    }
}
