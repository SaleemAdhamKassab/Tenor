using System.ComponentModel.DataAnnotations;

namespace Tenor.Models
{
    public class Level
    {
        [Key]
        public int Id { get; set; }
        [Required, StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 char")]
        public string Name { get; set; }
        [Required]
        public string DataType { get; set; }
        public bool IsFilter { get; set; }
        public bool IsLevel { get; set; }
        public virtual ICollection<DimensionLevel> DimensionLevels { get; set; }
        public virtual ICollection<ReportLevel> ReportLevels { get; set; }


    }
}
