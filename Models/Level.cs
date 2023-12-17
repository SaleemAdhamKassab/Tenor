using System.ComponentModel.DataAnnotations;

namespace Tenor.Models
{
    public class Level
    {
        [Key]
        public int Id { get; set; }
        [Required, StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 char")]
        public string Name { get; set; }

        public virtual ICollection<DeviceLevel> DeviceLevels { get; set; }

    }
}
