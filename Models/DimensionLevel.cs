using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tenor.Models
{
    public class DimensionLevel
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Dimension")]
        public int DimensionId {  get; set; }
        [ForeignKey("Level")]
        public int LevelId { get; set; }
        public string ColumnName { get;set; }
        public string OrderBy { get; set;}
        [ForeignKey("Parent")]
        public int ? ParentId { get; set; } //self join

        public virtual Dimension Dimension { get; set; }
        public virtual Level Level { get; set; }
        public virtual DimensionLevel  Parent { get; set; }
        public virtual ICollection<DimensionLevel> Childs { get; set; }
        public virtual ICollection<ReportFilter> ReportFilters { get; set; }

    }
}
