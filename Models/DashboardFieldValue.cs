using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Tenor.Models
{
    public class DashboardFieldValue
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Dashboard")]
        public int DashboardId { get; set; }
        [ForeignKey("DashboardField")]
        public int DashboardFieldId { get; set; }
        public string? FieldValue { get; set; }

        public virtual Dashboard Dashboard { get; set; }
        public virtual DashboardField DashboardField { get; set; }
    }
}
