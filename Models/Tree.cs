using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tenor.Models
{
    public class Tree
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("DeviceLevel")]
        public int DeviceLevelId { get;set; }
        [ForeignKey("Parent")]
        public int? ParentId { get;set; }
         
        public virtual DeviceLevel DeviceLevel { get; set; }
        public virtual Tree Parent { get; set; }
        public virtual ICollection<Tree> Childs { get; set; }
    }
}
