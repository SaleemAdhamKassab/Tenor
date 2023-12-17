using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tenor.Models
{
    public class DeviceLevel
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Device")]        
        public int DeviceId { get;set; }
        [ForeignKey("Level")]
        public int LevelId { get; set; }


        public virtual Device Device { get; set; }
        public virtual Level Level { get; set; }
    }
}
