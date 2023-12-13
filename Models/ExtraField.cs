using System.ComponentModel.DataAnnotations;
using static Tenor.Helper.Constant;

namespace Tenor.Models
{
    public class ExtraField
    {
        [Key]
        public int Id { get; set; }
        [Required, StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 char")]
        public string Name { get; set; }
        [Required]
        public fieldTypes Type { get; set; }
        public string? Content { get; set; }
        public string? Url { get; set; }


        public virtual ICollection<KpiField> KpiFields { get; set; }
        public virtual ICollection<CounterField> CounterFields { get; set; }
        public virtual ICollection<ReportField> ReportFields { get; set; }
        public virtual ICollection<DashboardField> DashboardFields { get; set; }
        public virtual ICollection<SubsetField> SubsetFields { get; set; }

    }
}
