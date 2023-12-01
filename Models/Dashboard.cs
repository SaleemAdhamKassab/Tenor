using System.ComponentModel.DataAnnotations;

namespace Tenor.Models
{
    public class Dashboard
    {
        [Key]
        public int Id { get; set; }
        [Required, StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 char")]
        public string Name { get; set; }

        public virtual ICollection<DashboardFieldValue> DashboardFieldValues { get; set; }
    }
}
