using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tenor.Models
{
    public class DimensionLevel
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("dimension")]
        public int DimensionId {  get; set; }
        [ForeignKey("level")]
        public int LevelId { get; set; }
        public string ColumnName { get;set; }
        public string OrderBy { get; set;}
        [ForeignKey("parent")]
        public int ? ParentId { get; set; } //self join

        public virtual Dimension dimension { get; set; }
        public virtual Level level { get; set; }
        public virtual DimensionLevel  parent { get; set; }
        public virtual ICollection<DimensionLevel> childs { get; set; }

    }
}
