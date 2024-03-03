using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tenor.Models
{
    public class Kpi
    {
        [Key]
        public int Id { get; set; }
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 char")]
        [Required]
        public string Name { get; set; }
        [ForeignKey("Operation")]
        public int OperationId { get; set; }
        [ForeignKey("Device")]
        public int? DeviceId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public bool IsPublic { get; set; }
        public string? ModifyBy { get; set; }
        public DateTime ? ModifyDate { get; set; } =null;
        public string? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; } = null;
        public virtual Operation Operation { get; set; }
        public virtual Device? Device { get; set; }
        public virtual ICollection<KpiFieldValue> KpiFieldValues { get; set; }
    }
}