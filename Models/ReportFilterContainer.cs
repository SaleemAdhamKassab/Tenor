using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Tenor.Helper.Constant;

namespace Tenor.Models
{
    public class ReportFilterContainer
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Report")]
        [Required]
        public int ReportId { get; set; }
        [Required]
        public enLogicalOperator LogicalOperator { get; set; }

        public virtual Report Report { get; set; }
        public virtual ICollection<ReportFilter> ReportFilters { get; set; }


        public ReportFilterContainer(int id,int reportId , List<ReportFilter> reportFilters)
        {
            Id=id;
            ReportId=reportId;
            ReportFilters=reportFilters;
        }

        public ReportFilterContainer() { }

    }
}
