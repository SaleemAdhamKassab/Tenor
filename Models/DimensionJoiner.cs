using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tenor.Models
{
    public class DimensionJoiner
    {
        [Key]
        public int Id { get; set; }
        public string PkName { get; set; }
        public string FkName { get;set; }
        [ForeignKey("Dimension")]
        public int DimensionId { get; set; }

        public virtual Dimension Dimension { get; set; }
    }
}
